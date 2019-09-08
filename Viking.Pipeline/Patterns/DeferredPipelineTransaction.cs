using System;
using System.Collections.Generic;

namespace Viking.Pipeline.Patterns
{
    public sealed class DeferredPipelineTransaction : IPipelineTransaction
    {
        private List<DeferredTransactionPart> Parts { get; } = new List<DeferredTransactionPart>();
        private bool IsCommitted { get; set; }
        public DeferredPipelineTransaction(IDeferredTransactionControl control)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
            Control.Register(this);
        }

        public IDeferredTransactionControl Control { get; }

        public PipelineTransactionResult Commit()
        {
            AssertNotCommitted();
            IsCommitted = true;
            var result = Control.Commit(this, Parts);
            Parts.Clear();

            return result;
        }

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
