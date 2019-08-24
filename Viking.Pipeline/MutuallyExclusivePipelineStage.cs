using System;
using System.Linq;

namespace Viking.Pipeline
{
    public class MutuallyExclusivePipelineStage<TValue> : IPipelineStage<TValue>
    {
        public MutuallyExclusivePipelineStage(IPipelineStage<TValue> input, params IPipelineStage[] mutuallyExclusiveWith)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            MutuallyExclusiveWith = mutuallyExclusiveWith ?? throw new ArgumentNullException(nameof(mutuallyExclusiveWith));
            Name = $"'{Input}' mutually exclusive with {{{string.Join(", ", MutuallyExclusiveWith.Select(stage => $"'{stage.Name}'"))}}}";
            this.AddDependencies(input);
            this.AddDependencies(mutuallyExclusiveWith);
        }

        public string Name { get; }
        public IPipelineStage<TValue> Input { get; }
        public IPipelineStage[] MutuallyExclusiveWith { get; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            if (MutuallyExclusiveWith.Any(invalidator.IsInvalidated))
                invalidator.Revalidate(this);
            else
                invalidator.InvalidateAllDependentStages(this);
        }

        public override string ToString() => Name;
    }
}
