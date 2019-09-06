using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Viking.Pipeline.Patterns
{
    public enum ConcurrentTransactionType
    {
        /// <summary>
        /// Concurrent transactions will always be committed, in an undefined order. Transactions as a whole will be kept intact.
        /// </summary>
        AlwaysCommit,
        /// <summary>
        /// The concurrent transaction is rolled back if an interleaving operation was found. Transactions can then be "redone" if needed.
        /// </summary>
        RollbackOnConcurrentUpdate
    }

    /// <summary>
    /// Provides basic transaction control.
    /// </summary>
    public class ConcurrentTransactionControl : IConcurrentTransactionControl
    {
        private long _timestamp = 0;

        private long LastUpdatedTimestamp { get; set; } = -1;
        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        public ConcurrentTransactionType TransactionType { get; }

        /// <summary>
        /// Creates a new <see cref="ConcurrentTransactionControl"/> with the specified transaction behavior.
        /// </summary>
        /// <param name="transactionType">The transaction behavior.</param>
        public ConcurrentTransactionControl(ConcurrentTransactionType transactionType)
        {
            TransactionType = transactionType;
        }

        public IPipelineTransaction CreateTransaction() => new ConcurrentPipelineTransaction(this);

        public long GetTimestamp() => Interlocked.Increment(ref _timestamp);

        public PipelineTransactionCommitResult Commit(IEnumerable<ConcurrentTransactionPart> parts)
        {
            var res = parts.ToList();
            if (res.Count <= 0)
                return PipelineTransactionCommitResult.Success;

            lock (this)
            {
                if (TransactionType == ConcurrentTransactionType.RollbackOnConcurrentUpdate && HasConcurrentCommit(res))
                    return PipelineTransactionCommitResult.Failed;

                return CommitTransaction(res);
            }
        }

        private bool HasConcurrentCommit(List<ConcurrentTransactionPart> res)
        {
            var start = res.Min(part => part.Timestamp);
            var end = res.Max(part => part.Timestamp);

            var hadConcurrentTransactionCommit = start < LastUpdatedTimestamp;

            LastUpdatedTimestamp = Math.Max(end, LastUpdatedTimestamp);

            return hadConcurrentTransactionCommit;
        }

        private PipelineTransactionCommitResult CommitTransaction(List<ConcurrentTransactionPart> res)
        {
            var commits = new Dictionary<IPipelineStage, ConcurrentTransactionPart>();

            foreach(var part in res)
            {
                if(!commits.TryGetValue(part.Stage, out var commit))
                {
                    commits.Add(part.Stage, part);
                    continue;
                }

                if (commit.Timestamp < part.Timestamp)
                    commits[part.Stage] = part;
            }

            var stagesToInvalidate = new List<IPipelineStage>();
            foreach(var part in commits.Values.OrderBy(p => p.Timestamp))
            {
                if (part.Action())
                    stagesToInvalidate.Add(part.Stage);
            }

            PipelineCore.Invalidate(stagesToInvalidate);
            return PipelineTransactionCommitResult.Success;
        }
    }
}
