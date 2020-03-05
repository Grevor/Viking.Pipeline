using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Makes a constant value available to the pipeline.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class ConstantPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="ConstantPipelineStage{TValue}"/> with the specified value.
        /// </summary>
        /// <param name="constant">The constant.</param>
        public ConstantPipelineStage(TValue constant) : this(constant?.ToString() ?? "<null>", constant) { }
        /// <summary>
        /// Creates a new <see cref="ConstantPipelineStage{TValue}"/> with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the constant.</param>
        /// <param name="constant">The constant.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null.</exception>
        public ConstantPipelineStage(string name, TValue constant)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = constant;
        }

        public string Name { get; }
        /// <summary>
        /// The constant value.
        /// </summary>
        public TValue Value { get; }

        public TValue GetValue() => Value;

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        /// <summary>
        /// Converts any value to a pipeline constant.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator ConstantPipelineStage<TValue>(TValue value) => new ConstantPipelineStage<TValue>(value);
        /// <summary>
        /// Converts a pipeline constant to its contained constant.
        /// </summary>
        /// <param name="stage">The stage.</param>
        public static implicit operator TValue(ConstantPipelineStage<TValue> stage) => stage.Value;

        public override string ToString() => FormattableString.Invariant($"Constant value '{Name}': {Value}");
    }
}
