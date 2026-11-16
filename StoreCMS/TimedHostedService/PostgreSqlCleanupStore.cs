using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Treynessen.Database.Entities;

namespace Treynessen
{
    internal sealed class PostgreSqlCleanupStore : ICleanupStore
    {
        private readonly DbConnection connection;
        private readonly CleanupObservationContext observer;
        internal PostgreSqlCleanupStore(DbConnection connection):this(connection,CleanupObservationContext.Noop) { }
        internal PostgreSqlCleanupStore(DbConnection connection,CleanupObservationContext context)
        {this.connection=connection;observer=CleanupObservationContext.Active(context)?.Connection();}

        internal static Task<ICleanupStore> Open(string connectionString, CancellationToken token)
            =>Open(connectionString,token,CleanupObservationContext.Noop);

        internal static async Task<ICleanupStore> Open(string connectionString, CancellationToken token,CleanupObservationContext context)
        {
            var settings = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Pooling = false, Multiplexing = false, Enlist = false,
                Timeout = 2, CommandTimeout = 3, CancellationTimeout = 1000
            };
            var store = new PostgreSqlCleanupStore(new NpgsqlConnection(settings.ConnectionString),context);
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeout.CancelAfter(TimeSpan.FromSeconds(2));
                await store.connection.OpenAsync(timeout.Token).ConfigureAwait(false);
                return store;
            }
            catch { await store.DisposeAsync().ConfigureAwait(false); throw; }
        }

        private async Task<T> Transaction<T>(Func<DbTransaction,CleanupObservationContext,Task<T>> operation, CancellationToken token,CleanupOperation operationKind)
        {
            try{token.ThrowIfCancellationRequested();}
            catch(Exception error){observer?.Failure(CleanupObservationKind.OperationFailed,CleanupComponent.Store,error,CleanupCommitKnowledge.NotAttempted,token);throw;}
            observer?.Emit(CleanupObservationKind.TransactionBeginAttempt,CleanupComponent.Store,()=>new CleanupTransactionPayload(operationKind,CleanupCommitKnowledge.NotAttempted),token);
            DbTransaction transaction;
            try{transaction=await connection.BeginTransactionAsync(token).ConfigureAwait(false);}
            catch(Exception error){observer?.Failure(CleanupObservationKind.OperationFailed,CleanupComponent.Store,error,CleanupCommitKnowledge.NotAttempted,token);throw;}
            var telemetry=transaction==null?null:observer?.Transaction();
            var knowledge=CleanupCommitKnowledge.NotAttempted;
            telemetry?.Emit(CleanupObservationKind.TransactionBegan,CleanupComponent.Store,()=>new CleanupTransactionPayload(operationKind,knowledge),token);
            try
            {
            await using (var settings = Command(transaction, "SET LOCAL lock_timeout = '100ms'; SET LOCAL statement_timeout = '2s'"))
                await settings.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            telemetry?.Emit(CleanupObservationKind.OperationEntered,CleanupComponent.Store,()=>new CleanupTransactionPayload(operationKind,knowledge),token);
            T result = await operation(transaction,telemetry).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            telemetry?.Emit(CleanupObservationKind.CommitAttempt,CleanupComponent.Store,()=>new CleanupTransactionPayload(operationKind,knowledge),token);
            knowledge=CleanupCommitKnowledge.Unknown;
            try{await transaction.CommitAsync(token).ConfigureAwait(false);}
            catch(Exception error){telemetry?.Failure(CleanupObservationKind.CommitUnknown,CleanupComponent.Store,error,knowledge,token);throw;}
            knowledge=CleanupCommitKnowledge.ProviderAcknowledged;
            telemetry?.Emit(CleanupObservationKind.CommitReturned,CleanupComponent.Store,()=>new CleanupTransactionPayload(operationKind,knowledge),token);
            return result;
            }
            catch(Exception error){telemetry?.Failure(CleanupObservationKind.OperationFailed,CleanupComponent.Store,error,knowledge,token);throw;}
            finally
            {
                telemetry?.Emit(CleanupObservationKind.DisposalAttempt,CleanupComponent.Store,()=>new CleanupTransactionPayload(operationKind,knowledge),token);
                try{if(transaction!=null)await transaction.DisposeAsync();telemetry?.Emit(CleanupObservationKind.DisposalReturned,CleanupComponent.Store,()=>new CleanupTransactionPayload(operationKind,knowledge),token);}
                catch(Exception error){telemetry?.Failure(CleanupObservationKind.DisposalFailed,CleanupComponent.Store,error,knowledge,token);throw;}
            }
        }

        private DbCommand Command(DbTransaction transaction, string sql, object bound = null)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandTimeout = 3;
            command.CommandText = sql;
            if (bound != null)
            {
                var parameter = command.CreateParameter(); parameter.ParameterName = "bound"; parameter.Value = bound;
                command.Parameters.Add(parameter);
            }
            return command;
        }

        public Task<(int? Visitors, int? Pages)> VisitBounds(CancellationToken token) => Transaction(async (tx,telemetry) =>
        {
            await using var cmd = Command(tx, "SELECT (SELECT max(\"ID\") FROM public.\"Visitors\"), (SELECT max(\"ID\") FROM public.\"VisitedPages\")");
            await using var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
            await reader.ReadAsync(token).ConfigureAwait(false);
            return (reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0), reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1));
        }, token,CleanupOperation.VisitBounds);

        public Task<int> DeletePages(int bound, CancellationToken token) => Transaction(async (tx,telemetry) =>
        {
            await using var cmd = Command(tx,
                $"WITH batch AS (SELECT \"ID\" FROM public.\"VisitedPages\" WHERE \"ID\" <= @bound ORDER BY \"ID\" LIMIT {CleanupWork.PageBatch}) DELETE FROM public.\"VisitedPages\" AS p USING batch WHERE p.\"ID\" = batch.\"ID\"", bound);
            return await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }, token,CleanupOperation.DeletePages);

        public Task<int> DeleteVisitors(int bound, CancellationToken token) => Transaction(async (tx,telemetry) =>
        {
            // [Комментарий ИИ-агента]: Удаляем только родителей без детей, чтобы каскад не скрывал неограниченный объём порции.
            await using var cmd = Command(tx,
                $"WITH batch AS (SELECT v.\"ID\" FROM public.\"Visitors\" AS v WHERE v.\"ID\" <= @bound AND NOT EXISTS (SELECT 1 FROM public.\"VisitedPages\" AS p WHERE p.\"VisitorId\" = v.\"ID\") ORDER BY v.\"ID\" LIMIT {CleanupWork.VisitorBatch}) DELETE FROM public.\"Visitors\" AS v USING batch WHERE v.\"ID\" = batch.\"ID\"", bound);
            return await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }, token,CleanupOperation.DeleteVisitors);

        public Task<bool> HasVisitors(int bound, CancellationToken token) => Transaction(async (tx,telemetry) =>
        {
            await using var cmd = Command(tx, "SELECT EXISTS(SELECT 1 FROM public.\"Visitors\" WHERE \"ID\" <= @bound)", bound);
            return (bool)await cmd.ExecuteScalarAsync(token).ConfigureAwait(false);
        }, token,CleanupOperation.HasVisitors);

        public Task<int?> LogBound(CancellationToken token) => Transaction(async (tx,telemetry) =>
        {
            await using var cmd = Command(tx, "SELECT max(\"ID\") FROM public.\"AdminPanelLogs\"");
            object value = await cmd.ExecuteScalarAsync(token).ConfigureAwait(false);
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }, token,CleanupOperation.LogBound);

        public Task<IReadOnlyList<AdminPanelLog>> ReadLogs(int bound, int? after, CancellationToken token) => Transaction<IReadOnlyList<AdminPanelLog>>(async (tx,telemetry) =>
        {
            await using var cmd = Command(tx,
                $"SELECT \"ID\", \"UserId\", \"Time\", CASE WHEN octet_length(\"Info\") <= 16384 THEN \"Info\" ELSE NULL END FROM public.\"AdminPanelLogs\" WHERE \"ID\" <= @bound" +
                (after.HasValue ? " AND \"ID\" > @after" : "") + $" ORDER BY \"ID\" LIMIT {CleanupWork.LogBatch}", bound);
            if (after.HasValue) { var p = cmd.CreateParameter(); p.ParameterName = "after"; p.Value = after.Value; cmd.Parameters.Add(p); }
            var logs = new List<AdminPanelLog>();
            await using var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                if (reader.IsDBNull(3)) throw new InvalidDataException("Событие превышает предел архивирования; SQL сохранён.");
                logs.Add(new AdminPanelLog { ID = reader.GetInt32(0), UserId = reader.GetInt32(1), Time = reader.GetDateTime(2), Info = reader.GetString(3) });
            }
            return logs;
        }, token,CleanupOperation.ReadLogs);

        public Task DeleteLogs(IReadOnlyList<int> ids, CancellationToken token) => Transaction(async (tx,telemetry) =>
        {
            if (ids.Count == 0 || ids.Count > CleanupWork.LogBatch) throw new ArgumentOutOfRangeException(nameof(ids));
            var names = new List<string>();
            await using var cmd = Command(tx, "");
            for (int i = 0; i < ids.Count; i++)
            {
                string name = "id" + i; names.Add("@" + name);
                var p = cmd.CreateParameter(); p.ParameterName = name; p.Value = ids[i]; cmd.Parameters.Add(p);
            }
            cmd.CommandText = "DELETE FROM public.\"AdminPanelLogs\" WHERE \"ID\" IN (" + string.Join(",", names) + ")";
            telemetry?.Emit(CleanupObservationKind.DeleteCommandAttempt,CleanupComponent.Store,()=>new CleanupIdsPayload(ids),token);
            int affected=await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            telemetry?.Emit(CleanupObservationKind.DeleteCommandReturned,CleanupComponent.Store,()=>new CleanupDeletePayload(ids,affected),token);
            return affected;
        }, token,CleanupOperation.DeleteLogs);

        public ValueTask DisposeAsync() => connection.DisposeAsync();
    }
}
