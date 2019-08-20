using System.Collections.Generic;

namespace Viking.Pipeline
{
    public interface IPipelineInvalidator
    {
        bool Invalidate(IPipelineStage stage);
        void InvalidateAllDependentStages(IPipelineStage stage);

        bool Revalidate(IPipelineStage stage);

        bool IsInvalidated(IPipelineStage stage);

        IEnumerable<IPipelineStage> AllInvalidatedStages { get; }
    }
}
