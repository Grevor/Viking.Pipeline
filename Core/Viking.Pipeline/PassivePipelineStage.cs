using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Pacifies a stage, causing it to never propagate.
    /// </summary>
    /// <typeparam name="TValue">The type of output.</typeparam>
    public sealed class PassivePipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="PassivePipelineStage{TValue}"/> with the specified input to pacify.
        /// </summary>
        /// <param name="input">The stage to pacify.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is null.</exception>
        public PassivePipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Pacifier for: " + input.Name;
            this.AddDependencies(input);
        }

        /// <summary>
        /// The input which is pacified.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }
        public string Name { get; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) { }

        public override string ToString() => Name;
    }
}
