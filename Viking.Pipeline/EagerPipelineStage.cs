using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Eagerly gets values from upstream stages.
    /// </summary>
    /// <typeparam name="TValue">The output value type.</typeparam>
    public sealed class EagerPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="EagerPipelineStage{TValue}"/> with the specified input.
        /// </summary>
        /// <param name="input">The input to make eager evaluation of.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is null.</exception>
        public EagerPipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Eager stage for: " + input.Name;
            this.AddDependencies(input);
        }

        public string Name { get; }

        /// <summary>
        /// Gets the input to eagerly retrieve value from.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            Input.GetValue();
            invalidator.InvalidateAllDependentStages(this);
        }

        public override string ToString() => Name;
    }
}
