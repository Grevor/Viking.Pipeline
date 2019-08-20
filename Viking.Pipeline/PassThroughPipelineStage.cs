using System;

namespace Viking.Pipeline
{
    public class PassThroughPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public PassThroughPipelineStage(string name, IPipelineStage<TValue> input)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            this.AddDependencies(input);
        }

        public string Name { get; }
        public IPipelineStage<TValue> Input { get; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);
    }
}
