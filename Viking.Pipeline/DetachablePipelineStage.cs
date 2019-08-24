using System;

namespace Viking.Pipeline
{
    public class DetachablePipelineStage<TValue> : PassThroughPipelineStage<TValue>
    {
        public DetachablePipelineStage(IPipelineStage<TValue> input) : base("Detacher for: " + (input?.Name ?? ""), input)
        {
        }

        public void DetachFromPipeline() => this.UnlinkAllDependencies();

        public override string ToString() => FormattableString.Invariant($"Detachable Stage - {Name}");
    }
}
