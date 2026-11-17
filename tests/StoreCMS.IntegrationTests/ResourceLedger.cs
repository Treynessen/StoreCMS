using System.Text.Json.Serialization;

namespace StoreCMS.IntegrationTests;

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum CreationState { Planned, ExpectedAbsent, CreateStarted, CreateAcknowledged, CreatedConfirmed, UnknownCreateOutcome }
[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum DeletionState { DropNotAttempted, DropStarted, DropAcknowledged, AbsentReadbackConfirmed, UnknownDropOutcome }

internal sealed record LedgerEvent(DateTime Utc, string Phase, string Code, CreationState Creation, DeletionState Deletion);

internal sealed class ResourceLedger(string runId, string kind, string name, string? owner = null, uint? ownerOid = null)
{
    public string RunId { get; } = runId;
    public string Kind { get; } = kind;
    public string Name { get; } = name;
    public string? ExpectedOwner { get; } = owner;
    public uint? OwnerOid { get; } = ownerOid;
    public uint? Oid { get; private set; }
    public CreationState Creation { get; private set; }
    public DeletionState Deletion { get; private set; }
    public bool Preserved { get; private set; }
    public List<LedgerEvent> Events { get; } = [];

    internal void Note(string phase, string code = "") =>
        Events.Add(new(DateTime.UtcNow, phase, code, Creation, Deletion));
    internal void BeginCreate()
    {
        if (Creation != CreationState.Planned) throw new InvalidOperationException("CreateRetryForbidden");
        Creation = CreationState.CreateStarted;
        Note("CreateStarted");
    }
    internal void AcknowledgeCreate()
    {
        if (Creation != CreationState.CreateStarted) throw new InvalidOperationException("UnexpectedCreateAcknowledgment");
        Creation = CreationState.CreateAcknowledged;
        Note("CreateAcknowledged");
    }
    internal void RecordOid(uint oid)
    {
        if (Creation != CreationState.CreateAcknowledged || oid == 0 || (Oid.HasValue && Oid != oid))
            throw new InvalidOperationException("IdentityMismatch");
        Oid = oid;
        Note("OidObserved");
    }
    internal void ConfirmCreated()
    {
        if (Creation != CreationState.CreateAcknowledged || !Oid.HasValue)
            throw new InvalidOperationException("IdentityMismatch");
        Creation = CreationState.CreatedConfirmed;
        Note("CreatedConfirmed");
    }
    internal void ExpectAbsent()
    {
        if (Creation != CreationState.Planned) throw new InvalidOperationException("UnexpectedAbsentState");
        Creation = CreationState.ExpectedAbsent;
        Note("ExpectedAbsent");
    }
    internal void BeginDrop()
    {
        if (Creation != CreationState.CreatedConfirmed || Deletion != DeletionState.DropNotAttempted || Preserved)
            throw new InvalidOperationException("DropRetryForbidden");
        Deletion = DeletionState.DropStarted;
        Note("DropStarted");
    }
    internal void AcknowledgeDrop()
    {
        if (Deletion != DeletionState.DropStarted) throw new InvalidOperationException("UnexpectedDropAcknowledgment");
        Deletion = DeletionState.DropAcknowledged;
        Note("DropAcknowledged");
    }
    internal void ConfirmAbsent()
    {
        if (Deletion != DeletionState.DropAcknowledged) throw new InvalidOperationException("AbsentWithoutDropAcknowledgment");
        Deletion = DeletionState.AbsentReadbackConfirmed;
        Note("AbsentReadbackConfirmed");
    }
    internal void Preserve()
    {
        if (Creation == CreationState.CreateStarted) Creation = CreationState.UnknownCreateOutcome;
        if (Deletion == DeletionState.DropStarted) Deletion = DeletionState.UnknownDropOutcome;
        Preserved = Deletion != DeletionState.AbsentReadbackConfirmed;
        Note("Stopped", Preserved ? "Preserved" : "");
    }
}
