namespace Viking.Pipeline
{
    public class DetachablePipelineStage<TValue> : PassThroughPipelineStage<TValue>
    {
        public DetachablePipelineStage(IPipelineStage<TValue> input) : base("Detacher for: " + (input?.Name ?? ""), input)
        {
        }

        public void DetachFromPipeline() => this.UnlinkAllDependencies();
    }
}
