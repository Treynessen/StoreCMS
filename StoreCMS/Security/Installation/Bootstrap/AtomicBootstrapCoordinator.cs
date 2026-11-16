using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Treynessen.Database;

namespace Treynessen.Security.Installation.Bootstrap;

internal sealed class AtomicBootstrapCoordinator
{
    private readonly IBootstrapBackend backend;
    private readonly InstallationTarget target;
    private readonly Func<DateTime> clock;
    internal AtomicBootstrapCoordinator(IBootstrapBackend backend, InstallationTarget target, Func<DateTime> utcClock)
    { this.backend = backend; this.target = target; clock = utcClock; }

    internal async Task<BootstrapResult> InstallAsync(InitialInstallationPermission permission, CancellationToken cancellationToken = default,
        string initialPassword = null)
    {
        if (permission != InitialInstallationPermission.AllowInitialInstallation) return new(BootstrapState.AuthorizationRequired, "ExplicitPermissionRequired");
        if (target == null || target.Schema != "public" || string.IsNullOrWhiteSpace(target.Database) || string.IsNullOrWhiteSpace(target.Role))
            return new(BootstrapState.SchemaRejected, "InvalidTarget");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(30));
        BootstrapPlan plan;
        try
        {
            var preflight = await backend.InspectAsync(deadline.Token);
            if (preflight?.State != InstallationState.NotInitialized) return Map(preflight);
            if (!DatabaseInteraction.CorrectPassword(initialPassword))
                return new(BootstrapState.AuthorizationRequired, "InitialAdminPasswordRequiredOrInvalid");
            plan = BootstrapPlan.Create(clock(), initialPassword);
        }
        catch (Exception error) { return BeforeCommitFailure(error); }

        IBootstrapTransaction transaction = null;
        var commit = new CommitProgress();
        BootstrapResult result;
        try
        {
            transaction = await backend.BeginAsync(deadline.Token);
            result = await ExecuteProtectedAsync(transaction, plan, commit, deadline.Token);
        }
        catch (Exception error)
        {
            result = commit.Attempted ? new(BootstrapState.UnknownCommitOutcome, "CommitNotAcknowledged") : BeforeCommitFailure(error);
        }
        bool? rolledBack = null;
        bool cleanupFailed = false;
        if (transaction != null)
        {
            // [Комментарий ИИ-агента]: После отправки COMMIT нельзя объявлять откат по сетевой ошибке; освобождение ресурсов не доказывает исход установки.
            if (!commit.Attempted)
            {
                using var cleanupDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try { await transaction.RollbackAsync(cleanupDeadline.Token); rolledBack = true; }
                catch (Exception) { rolledBack = false; cleanupFailed = true; }
            }
            try { await transaction.DisposeAsync(); }
            catch (Exception) { cleanupFailed = true; }
        }
        result = result with { RollbackConfirmed = rolledBack, CleanupFailed = cleanupFailed };
        if (result.State == BootstrapState.UnknownCommitOutcome)
        {
            // [Комментарий ИИ-агента]: Одно независимое чтение после закрытия соединения, без повторной вставки даже при наблюдаемой пустоте.
            using var inspectionDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try { result = result with { ObservedState = (await backend.InspectAsync(inspectionDeadline.Token))?.State ?? InstallationState.Unavailable }; }
            catch (Exception) { result = result with { ObservedState = InstallationState.Unavailable }; }
        }
        return result;
    }

    private async Task<BootstrapResult> ExecuteProtectedAsync(IBootstrapTransaction transaction, BootstrapPlan plan, CommitProgress commit, CancellationToken token)
    {
        var identity = await transaction.ReadIdentityAsync(token);
        if (identity == null || identity.Database != target.Database || identity.Role != target.Role || identity.Schema != "public" || identity.ServerMajor != 18)
            return new(BootstrapState.SchemaRejected, "TargetMismatch");
        if (identity.ReadOnly) return new(BootstrapState.Unavailable, "WriteTransactionRequired");
        if (!await transaction.HasLockPermissionAsync(token)) return new(BootstrapState.SchemaRejected, "AdvisoryPermissionMissing");
        if (!await transaction.TryAdvisoryLockAsync(token)) return new(BootstrapState.InstallingBusy, "AdvisoryLockBusy");
        await transaction.LockTablesAsync(BootstrapPlan.TableOrder, token);
        var fresh = await transaction.ValidateAfterLocksAsync(token);
        if (fresh?.State != InstallationState.NotInitialized) return Map(fresh);
        int roleId = await transaction.InsertRoleAsync(plan, token);
        int userId = await transaction.InsertUserAsync(plan, roleId, token);
        await transaction.InsertMarkerAsync(plan, userId, roleId, token);
        var data = await transaction.ReadDataAsync(token);
        if (!plan.Matches(data, roleId, userId)
            || InstallationEvaluator.EvaluateData(data, clock()).State != InstallationState.InstalledPasswordChangeRequired)
            return new(BootstrapState.RecoveryRequired, "PostInsertInvariantFailed");
        token.ThrowIfCancellationRequested();
        commit.Attempted = true;
        await transaction.CommitAsync(token);
        return new(BootstrapState.InstallationCompleted, "CommitAcknowledged");
    }

    private static BootstrapResult Map(InstallationResult result) => result?.State switch
    {
        InstallationState.InstalledReady or InstallationState.InstalledPasswordChangeRequired => new(BootstrapState.AlreadyInstalled, "InstallationPreserved"),
        InstallationState.RecoveryRequired => new(BootstrapState.RecoveryRequired, "InstallationRequiresReview"),
        InstallationState.SchemaRejected => new(BootstrapState.SchemaRejected, "SchemaNotAccepted"),
        _ => new(BootstrapState.Unavailable, "InspectionUnavailable")
    };
    private static BootstrapResult BeforeCommitFailure(Exception error) => error is PostgresException pg ? pg.SqlState switch
    {
        "55P03" or "40P01" or "40001" => new(BootstrapState.InstallingBusy, "ConcurrentOperation"),
        "42501" => new(BootstrapState.SchemaRejected, "PermissionDenied"),
        _ => new(BootstrapState.Unavailable, "BeforeCommitFailure")
    } : new(BootstrapState.Unavailable, "BeforeCommitFailure");
    private sealed class CommitProgress { internal bool Attempted; }
}
