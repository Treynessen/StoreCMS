using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Treynessen;
internal enum CleanupObservationKind
{
    CallStarted,LogBound,BatchSelected,PublisherInvocation,PublisherReturnedIds,IdsValidated,DeleteInvocation,DeleteReturned,CallCompleted,CallFailed,Cancelled,
    TransactionBeginAttempt,TransactionBegan,OperationEntered,DeleteCommandAttempt,DeleteCommandReturned,CommitAttempt,CommitReturned,CommitUnknown,
    DisposalAttempt,DisposalReturned,DisposalFailed,OperationFailed,PublisherEntered,WriteStarted,WriteCompleted,FlushStarted,FlushCompleted,
    MoveStarted,MoveCompleted,PublicationCompleted,PublisherReturned,PublisherFailed
}
internal enum CleanupComponent { Work,Store,Publisher }
internal enum CleanupOperation { None,VisitBounds,DeletePages,DeleteVisitors,HasVisitors,LogBound,ReadLogs,DeleteLogs }
internal enum CleanupCommitKnowledge { NotAttempted,ProviderAcknowledged,Unknown }
internal enum CleanupFailure { Cancelled,FileIo,Database,Other }
internal abstract record CleanupPayload;
internal sealed record CleanupEmptyPayload:CleanupPayload;
internal sealed record CleanupIdsPayload:CleanupPayload
{
    internal ImmutableArray<int> Ids { get; }
    internal CleanupIdsPayload(IEnumerable<int> ids){Ids=ImmutableArray.CreateRange(ids);}
}
internal sealed record CleanupBoundPayload(int? Bound):CleanupPayload;
internal sealed record CleanupDeletePayload:CleanupPayload
{
    internal ImmutableArray<int> RequestedIds { get; }
    internal int AffectedRows { get; }
    internal CleanupDeletePayload(IEnumerable<int> requestedIds,int affectedRows){RequestedIds=ImmutableArray.CreateRange(requestedIds);AffectedRows=affectedRows;}
}
internal sealed record CleanupTransactionPayload(CleanupOperation Operation,CleanupCommitKnowledge Commit):CleanupPayload;
internal sealed record CleanupFailurePayload(CleanupFailure Failure,CleanupCommitKnowledge Commit):CleanupPayload;
internal sealed record CleanupFilePayload(string RelativePath,long Length,string SerializedBytesSha256):CleanupPayload;
internal sealed record CleanupObservation(int Version,Guid RunId,Guid CallId,string ScenarioId,string PlanSeal,Guid? BatchId,Guid? PublicationId,
    Guid? ConnectionHandle,Guid? TransactionHandle,CleanupComponent Component,CleanupObservationKind Kind,long Sequence,long MonotonicTimestamp,
    DateTimeOffset Timestamp,Guid SourceMvid,bool CancellationRequested,CleanupPayload Payload,[property:System.Text.Json.Serialization.JsonIgnore]object IssuerToken);
internal sealed class CleanupObservationContext
{
    internal const int ContractVersion=1;
    internal static CleanupObservationContext Noop { get; }=new CleanupObservationContext();
    private sealed class State
    {
        internal readonly object Sync=new object();
        internal Guid RunId,CallId;
        internal string ScenarioId,PlanSeal;
        internal Func<CleanupObservation,bool> Sink;
        internal long Sequence;
        internal int Fault;
        internal Guid? Batch;
        internal readonly object IssuerToken=new object();
    }
    private readonly State state;
    private readonly Guid? connection,transaction,publication,batch;
    private CleanupObservationContext(){ }
    internal CleanupObservationContext(Guid runId,Guid callId,string scenarioId,string planSeal,Func<CleanupObservation,bool> sink)
    {
        if(runId==Guid.Empty||callId==Guid.Empty||scenarioId!="PG-08"||planSeal==null||planSeal.Length!=64||sink==null)throw new ArgumentException("Observation scope");
        state=new State{RunId=runId,CallId=callId,ScenarioId=scenarioId,PlanSeal=planSeal,Sink=sink};
    }
    private CleanupObservationContext(CleanupObservationContext parent,Guid? connection,Guid? transaction,Guid? publication,Guid? batch)
    {state=parent.state;this.connection=connection;this.transaction=transaction;this.publication=publication;this.batch=batch;}
    internal bool Faulted=>state!=null&&Volatile.Read(ref state.Fault)!=0;
    internal long LastSequence=>state==null?0:Interlocked.Read(ref state.Sequence);
    internal object IssuerToken=>state?.IssuerToken;
    internal static CleanupObservationContext Active(CleanupObservationContext context)=>context?.state==null?null:context;
    internal void NextBatch()
    {try{lock(state.Sync){state.Batch=Guid.NewGuid();}}catch{MarkFault();}}
    internal CleanupObservationContext Connection()=>Child(Guid.NewGuid(),null,null,null);
    internal CleanupObservationContext Transaction()=>Child(connection,Guid.NewGuid(),null,state.Batch);
    internal CleanupObservationContext Publication()=>Child(connection,null,Guid.NewGuid(),state.Batch);
    private CleanupObservationContext Child(Guid? c,Guid? t,Guid? p,Guid? b)
    {try{return new CleanupObservationContext(this,c,t,p,b);}catch{MarkFault();return null;}}
    private void MarkFault(){if(state!=null)Interlocked.Exchange(ref state.Fault,1);}
    internal CleanupFilePayload SerializedFile(string root,string path,MemoryStream contents)
    {
        try{return new CleanupFilePayload(Path.GetRelativePath(root,path),contents.Length,Convert.ToHexString(SHA256.HashData(contents.GetBuffer().AsSpan(0,checked((int)contents.Length)))));}
        catch{MarkFault();return null;}
    }
    internal void Emit(CleanupObservationKind kind,CleanupComponent component,Func<CleanupPayload> payload,CancellationToken token)
    {
        if(state==null)return;
        try
        {
            lock(state.Sync)
            {
                long seq=++state.Sequence;
                Guid? currentBatch=kind is CleanupObservationKind.CallStarted or CleanupObservationKind.CallCompleted or CleanupObservationKind.CallFailed?null:batch??state.Batch;
                var observation=new CleanupObservation(ContractVersion,state.RunId,state.CallId,state.ScenarioId,state.PlanSeal,currentBatch,publication,connection,transaction,
                    component,kind,seq,Stopwatch.GetTimestamp(),DateTimeOffset.UtcNow,typeof(CleanupObservationContext).Module.ModuleVersionId,token.IsCancellationRequested,payload(),state.IssuerToken);
                if(!state.Sink(observation))MarkFault();
            }
        }
        catch{MarkFault();}
    }
    internal void Failure(CleanupObservationKind kind,CleanupComponent component,Exception error,CleanupCommitKnowledge commit,CancellationToken token)
        =>Emit(error is OperationCanceledException?CleanupObservationKind.Cancelled:kind,component,
            ()=>new CleanupFailurePayload(error is OperationCanceledException?CleanupFailure.Cancelled:error is IOException?CleanupFailure.FileIo:error is System.Data.Common.DbException?CleanupFailure.Database:CleanupFailure.Other,commit),token);
}
