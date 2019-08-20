using System;

namespace Viking.Pipeline
{
    public class ReactionPipelineStage : IPipelineStage
    {
        public ReactionPipelineStage(Action reaction, params IPipelineStage[] stages)
        {
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
            Stages = stages ?? throw new ArgumentNullException(nameof(stages));
            Name = "Reaction to ";
        }

        public string Name { get; }
        public Action Reaction { get; }
        public IPipelineStage[] Stages { get; }

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            invalidator.InvalidateAllDependentStages(this);
            Reaction();
        }
    }

    public partial class ReactionPipelineStage<TInput1> : IPipelineStage<TInput1>
    {
        public TInput1 GetValue() => Input1.GetValue();
    }
}
