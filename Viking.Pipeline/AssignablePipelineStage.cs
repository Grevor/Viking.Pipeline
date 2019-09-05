using System;
using System.Collections.Generic;

namespace Viking.Pipeline
{
    /// <summary>
    /// Provides a value to the pipeline which can be changed at will.
    /// </summary>
    /// <typeparam name="TValue">The type of the provided value.</typeparam>
    public sealed class AssignablePipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="AssignablePipelineStage{TValue}"/> with the specified name and initial value, using the default <see cref="IEqualityComparer{T}"/> to check for equality.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="initial">The initial value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null.</exception>
        public AssignablePipelineStage(string name, TValue initial) : this(name,initial, EqualityComparer<TValue>.Default) { }
        /// <summary>
        /// Creates a new <see cref="AssignablePipelineStage{TValue}"/> with the specified name, initial value and comparer.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="initial">The initial value.</param>
        /// <param name="comparer">The comparer to use for equality comparison.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="comparer"/> is null.</exception>
        public AssignablePipelineStage(string name, TValue initial, IEqualityComparer<TValue> comparer)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = initial;
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public string Name { get; }
        /// <summary>
        /// Gets the current value provided by this stage.
        /// </summary>
        public TValue Value { get; private set; }
        /// <summary>
        /// Gets the comparer used by this stage to determine value equality.
        /// </summary>
        public IEqualityComparer<TValue> Comparer { get; }

        /// <summary>
        /// Sets the value provided by this stage.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the value changed.</returns>
        public bool SetValue(TValue value) => SetValue(value, true);
        /// <summary>
        /// Sets the value provided by this stage without invalidating the pipeline.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the value changed.</returns>
        public bool SetValueWithoutInvalidating(TValue value) => SetValue(value, false);
        private bool SetValue(TValue value, bool invalidate)
        {
            var valueChanged = !Comparer.Equals(Value, value);
            invalidate &= valueChanged;
            Value = value;
            if (invalidate)
                this.Invalidate();
            return valueChanged;
        }


        public TValue GetValue() => Value;
        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Assignable value '{Name}': {Value.ToString()}");
    }
}
