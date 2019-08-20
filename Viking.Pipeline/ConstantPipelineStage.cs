namespace Viking.Pipeline
{
    public class ConstantPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public ConstantPipelineStage(TValue constant) : this(constant?.ToString() ?? "<null>", constant) { }
        public ConstantPipelineStage(string name, TValue constant)
        {
            Name = name;
            Value = constant;
        }

        public string Name { get; }
        public TValue Value { get; }

        public TValue GetValue() => Value;

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public static implicit operator ConstantPipelineStage<TValue>(TValue value) => new ConstantPipelineStage<TValue>(value);
        public static implicit operator TValue(ConstantPipelineStage<TValue> stage)=> stage.Value;
    }
}
