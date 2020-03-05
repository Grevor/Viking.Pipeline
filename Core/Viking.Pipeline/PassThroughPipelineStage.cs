using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Stage which simply passes the value of the last stage through to the next.
    /// </summary>
    /// <typeparam name="TValue">The output value type.</typeparam>
    public class PassThroughPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="PassThroughPipelineStage{TValue}"/> with the specified name and input.
        /// </summary>
        /// <param name="name">The name of this stage.</param>
        /// <param name="input">The input to pass through.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="input"/> is null.</exception>
        public PassThroughPipelineStage(string name, IPipelineStage<TValue> input)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            this.AddDependencies(input);
        }

        public string Name { get; }
        /// <summary>
        /// Gets the input of this pass-through stage.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Pass-through - {Name}");
    }
}
