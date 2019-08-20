using System;
using System.Collections.Generic;

namespace Viking.Pipeline
{
    public class AssignablePipelineStage<TValue> : IPipelineStage<TValue>
    {
        public AssignablePipelineStage(string name, TValue initial) : this(name,initial, EqualityComparer<TValue>.Default) { }
        public AssignablePipelineStage(string name, TValue initial, IEqualityComparer<TValue> comparer)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = initial;
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public string Name { get; }
        public TValue Value { get; private set; }
        public IEqualityComparer<TValue> Comparer { get; }

        public void SetValue(TValue value) => SetValue(value, true);
        public void SetValueWithoutInvalidating(TValue value) => SetValue(value, false);
        private void SetValue(TValue value, bool invalidate)
        {
            var valueChanged = invalidate && !Comparer.Equals(Value, value);
            Value = value;
            if (valueChanged)
                this.Invalidate();
        }


        public TValue GetValue() => Value;
        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Assignable value '{Name}': {Value.ToString()}");
    }
}
