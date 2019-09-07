using System;

namespace Viking.Pipeline.Patterns
{
    public class DeferredTransactionPart
    {
        public DeferredTransactionPart(IPipelineStage stage, PipelineUpdateAction action, long timestamp)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Timestamp = timestamp;
        }

        public IPipelineStage Stage { get; }
        public PipelineUpdateAction Action { get; }
        public long Timestamp { get; }
    }
}
