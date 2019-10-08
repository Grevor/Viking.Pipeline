using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Provides control for concurrent transactions. Concurrent transactions are aggregated if they do not overlap.
    /// </summary>
    public sealed class AggregatingTransactionControl : IDeferredTransactionControl
    {
        private long _timestamp = 0;

        private HashSet<IPipelineTransaction> OngoingTransactions { get; } = new HashSet<IPipelineTransaction>();
        private Dictionary<IPipelineStage, DeferredTransactionPart> AggregatedTransaction { get; } = new Dictionary<IPipelineStage, DeferredTransactionPart>();

        /// <summary>
        /// Creates a new <see cref="AggregatingTransactionControl"/> with the specified transaction behavior.
        /// </summary>
        /// <param name="transactionType">The transaction behavior.</param>
        public AggregatingTransactionControl() { }

        /// <summary>
        /// Creates a new transaction. This transaction must be committed, or the system might deadlock.
        /// </summary>
        /// <returns></returns>
        public IPipelineTransaction CreateTransaction() => new DeferredPipelineTransaction(this);

        public void Register(DeferredPipelineTransaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            lock (this)
                OngoingTransactions.Add(transaction);
        }

        public void Cancel(DeferredPipelineTransaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            lock (this)
            {
                OngoingTransactions.Remove(transaction);
                if (DeregisterOngoing(transaction))
                    CommitTransaction(AggregatedTransaction.Values);
            }
        }

        public long GetTimestamp() => Interlocked.Increment(ref _timestamp);

        public PipelineTransactionResult Commit(DeferredPipelineTransaction transaction, IEnumerable<DeferredTransactionPart> parts)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));
            if (parts is null)
                throw new ArgumentNullException(nameof(parts));

            if (!OngoingTransactions.Contains(transaction))
                throw new ArgumentException("Transaction is not registered with this transaction control.", nameof(transaction));
            var res = parts.ToList();
            if (res.Count <= 0)
                return PipelineTransactionResult.Success;

            lock (this)
                return AggregateTransaction(transaction, res);
        }



        private PipelineTransactionResult AggregateTransaction(IPipelineTransaction transaction, List<DeferredTransactionPart> res)
        {
            var mustCommitAggregate = DeregisterOngoing(transaction);
            var mustRollback = res.Any(part => AggregatedTransaction.ContainsKey(part.Stage));

            if (mustRollback)
            {
                if (mustCommitAggregate)
                    CommitTransaction(AggregatedTransaction.Values);
                return PipelineTransactionResult.Failed;
            }
            else
            {
                foreach (var part in res)
                    AggregatedTransaction.Add(part.Stage, part);

                if (mustCommitAggregate)
                    return CommitTransaction(AggregatedTransaction.Values);
                else
                    return PipelineTransactionResult.PendingSuccess;
            }
        }

        private bool DeregisterOngoing(IPipelineTransaction transaction)
        {
            if (!OngoingTransactions.Remove(transaction))
                throw new ArgumentException("Transaction is not registered.");
            return OngoingTransactions.Count <= 0;
        }

        private static PipelineTransactionResult CommitTransaction(IEnumerable<DeferredTransactionPart> res)
        {
            var stagesToInvalidate = res.OrderBy(p => p.Timestamp).Where(p => p.Action()).Select(p => p.Stage).ToList();
            PipelineCore.Invalidate(stagesToInvalidate);
            return PipelineTransactionResult.Success;
        }
    }
}
