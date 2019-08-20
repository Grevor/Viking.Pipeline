using System;

namespace Viking.Pipeline
{
    public class EagerPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public EagerPipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Eager stage for: " + input.Name;
            this.AddDependencies(input);
        }

        public string Name { get; }

        public IPipelineStage<TValue> Input { get; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            Input.GetValue();
            invalidator.InvalidateAllDependentStages(this);
        }
    }
}
