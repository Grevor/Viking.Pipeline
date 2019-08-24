using System;

namespace Viking.Pipeline
{
    public class PassivePipelineStage<TValue> : IPipelineStage<TValue>
    {
        public PassivePipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Pacifier for: " + input.Name;
            this.AddDependencies(input);
        }

        public IPipelineStage<TValue> Input { get; }
        public string Name { get; }

        public TValue GetValue() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) { }

        public override string ToString() => Name;
    }
}
