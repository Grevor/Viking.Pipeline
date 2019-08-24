using System;
using System.Collections.Generic;
using System.Globalization;

namespace Viking.Pipeline
{
    public delegate bool EqualityCheck<T>(T a, T b);
    public class EqualityCheckerPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public EqualityCheckerPipelineStage(IPipelineStage<TValue> input) : this(input, EqualityComparer<TValue>.Default) { }
        public EqualityCheckerPipelineStage(IPipelineStage<TValue> input, IEqualityComparer<TValue> comparer) : this(input, comparer.Equals) { }
        public EqualityCheckerPipelineStage(IPipelineStage<TValue> input, EqualityCheck<TValue> comparer)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            Name = "Equality check for: " + input.Name;
            this.AddDependencies(input);
        }

        public IPipelineStage<TValue> Input { get; }
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

        public override string ToString() => FormattableString.Invariant($"Equality Checker - Comparer: {Comparer} LastValue: {(HasValue ? LastValue.ToString() : "<None yet>")}");
    }
}
