using System;
using System.Collections.Generic;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Provides a pipeline transaction which reports back to some <see cref="IDeferredTransactionControl"/>.
    /// </summary>
    public sealed class DeferredPipelineTransaction : IPipelineTransaction
    {
        private List<DeferredTransactionPart> Parts { get; } = new List<DeferredTransactionPart>();
        private bool IsCommitted { get; set; }

        /// <summary>
        /// Creates a new <see cref="DeferredPipelineTransaction"/> with the specified control.
        /// </summary>
        /// <param name="control">The control which to report back to.</param>
        public DeferredPipelineTransaction(IDeferredTransactionControl control)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
            Control.Register(this);
        }

        /// <summary>
        /// Gets the control in charge of this transaction.
        /// </summary>
        public IDeferredTransactionControl Control { get; }

        /// <summary>
        /// Commits this transaction.
        /// </summary>
        /// <returns>The result.</returns>
        public PipelineTransactionResult Commit()
        {
            AssertNotCommitted();
            IsCommitted = true;
            var result = Control.Commit(this, Parts);
            Parts.Clear();

            return result;
        }

        /// <summary>
        /// Adds the specified stage and update action as a part of the transaction.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="update">The update to perform.</param>
        /// <returns>This transaction.</returns>
        public IPipelineTransaction Update(IPipelineStage stage, PipelineUpdateAction update)
        {
            if (stage is null)
                throw new ArgumentNullException(nameof(stage));
            if (update is null)
                throw new ArgumentNullException(nameof(update));

            AssertNotCommitted();
            Parts.Add(new DeferredTransactionPart(stage, update, Control.GetTimestamp()));

            return this;
        }

        /// <summary>
        /// Cancels this transaction. The transaction will be invalid after this is called.
        /// </summary>
        public void Cancel()
        {
            AssertNotCommitted();
            IsCommitted = true;
            Parts.Clear();
            Control.Cancel(this);
        }

        private void AssertNotCommitted()
        {
            if (IsCommitted)
                throw new InvalidOperationException("This transaction has already committed.");
        }
    }
}
