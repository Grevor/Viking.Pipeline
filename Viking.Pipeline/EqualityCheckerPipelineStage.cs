using System;
using System.Collections.Generic;

namespace Viking.Pipeline
{
    /// <summary>
    /// Delegate that checks two values for equality.
    /// </summary>
    /// <typeparam name="T">The type of the objects.</typeparam>
    /// <param name="a">The first object.</param>
    /// <param name="b">The second object.</param>
    /// <returns>True if a and b are equal.</returns>
    public delegate bool EqualityCheck<T>(T a, T b);

    /// <summary>
    /// Checks for equality and stops propagation if found.
    /// </summary>
    /// <typeparam name="TValue">The type of the output.</typeparam>
    public sealed class EqualityCheckerPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="EqualityCheckerPipelineStage{TValue}"/> with the specified input, using the default <see cref="IEqualityComparer{T}"/> as comparer.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is null.</exception>
        public EqualityCheckerPipelineStage(IPipelineStage<TValue> input) : this(input, EqualityComparer<TValue>.Default) { }
        /// <summary>
        /// Creates a new <see cref="EqualityCheckerPipelineStage{TValue}"/> with the specified input and comparer.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="comparer">The comparer to use for equality.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> or <paramref name="comparer"/> is null.</exception>
        public EqualityCheckerPipelineStage(IPipelineStage<TValue> input, IEqualityComparer<TValue> comparer) : this(input, comparer.Equals) { }
        /// <summary>
        /// Creates a new <see cref="EqualityCheckerPipelineStage{TValue}"/> with the specified input and comparer.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="comparer">The comparer to use for equality.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> or <paramref name="comparer"/> is null.</exception>
        public EqualityCheckerPipelineStage(IPipelineStage<TValue> input, EqualityCheck<TValue> comparer)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            Name = "Equality check for: " + input.Name;
            this.AddDependencies(input);
        }

        /// <summary>
        /// The input to check for equality.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }
        /// <summary>
        /// The comparer used to check for equality.
        /// </summary>
        public EqualityCheck<TValue> Comparer { get; }
        public string Name { get; }

        private bool HasValue { get; set; }
        private TValue LastValue { get; set; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            var newValue = GetValue();
            var sameValue = HasValue && Comparer(LastValue, newValue);
            LastValue = newValue;
            HasValue = true;
            if (sameValue)
                invalidator.Revalidate(this);
            else
                invalidator.InvalidateAllDependentStages(this);
        }

        public override string ToString() => FormattableString.Invariant($"Equality Checker - Comparer: {Comparer} LastValue: {(HasValue ? LastValue : (object)"<None yet>")}");
    }
}
