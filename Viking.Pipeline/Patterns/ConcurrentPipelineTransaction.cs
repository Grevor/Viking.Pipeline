using System;
using System.Collections.Generic;

namespace Viking.Pipeline.Patterns
{
    public class ConcurrentPipelineTransaction : IPipelineTransaction
    {
        private List<ConcurrentTransactionPart> Parts { get; } = new List<ConcurrentTransactionPart>();
        public ConcurrentPipelineTransaction(IConcurrentTransactionControl control)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
        }

        public IConcurrentTransactionControl Control { get; }

        public PipelineTransactionCommitResult Commit()
        {
            var result = Control.Commit(Parts);
            Parts.Clear();

            return result;
        }

        public IPipelineTransaction Update(IPipelineStage stage, PipelineUpdateAction update)
        {
            Parts.Add(new ConcurrentTransactionPart(stage, update, Control.GetTimestamp()));

            return this;
        }
    }
}
