using System.Collections.Generic;

namespace Viking.Pipeline
{
    internal class PipelineInvalidator : IPipelineInvalidator
    {
        public HashSet<IPipelineStage> Invalidated { get; } = new HashSet<IPipelineStage>();
        public List<IPipelineStage> InvalidatedByThisStep { get; } = new List<IPipelineStage>();
        public List<IPipelineStage> DependentStages { get; private set; } = new List<IPipelineStage>();

        public IEnumerable<IPipelineStage> AllInvalidatedStages => Invalidated;

        public void PrepareForNextStage(List<IPipelineStage> dependent)
        {
            InvalidatedByThisStep.Clear();
            DependentStages = dependent;
        }

        public bool Invalidate(IPipelineStage stage)
        {
            if (Invalidated.Add(stage))
            {
                InvalidatedByThisStep.Add(stage);
                return true;
            }
            return false;
        }

        public void InvalidateAllDependentStages(IPipelineStage stage)
        {
            foreach (var dependent in DependentStages)
                Invalidate(dependent);
        }

        public bool IsInvalidated(IPipelineStage stage) => Invalidated.Contains(stage);

        public bool Revalidate(IPipelineStage stage) => Invalidated.Remove(stage);
    }
}
