using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    /// <summary>
    /// Conditionally propagates invalidations based on other stages invalidation state during propagation.
    /// </summary>
    /// <typeparam name="TValue">The output type.</typeparam>
    public sealed class MutuallyExclusivePipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="MutuallyExclusivePipelineStage{TValue}"/> with the specified input and mutually exclusive stages.
        /// </summary>
        /// <param name="input">The input stage.</param>
        /// <param name="mutuallyExclusiveWith">The stages which, if invalidated at the time of propagation, will cause this stage to stop propagation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> or <paramref name="mutuallyExclusiveWith"/> is null.</exception>
        public MutuallyExclusivePipelineStage(IPipelineStage<TValue> input, params IPipelineStage[] mutuallyExclusiveWith)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            MutuallyExclusiveWith = mutuallyExclusiveWith ?? throw new ArgumentNullException(nameof(mutuallyExclusiveWith));
            this.AddDependencies(input);
            this.AddDependencies(mutuallyExclusiveWith);
            Name = $"'{Input}' mutually exclusive with {{{string.Join(", ", MutuallyExclusiveWith.Select(stage => $"'{stage.Name}'"))}}}";
        }

        public string Name { get; }
        /// <summary>
        /// Gets the input stage.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }
        /// <summary>
        /// Gets all stages which will stop propagation of this stage if invalid.
        /// </summary>
        public IEnumerable<IPipelineStage> MutuallyExclusiveWith { get; }

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
