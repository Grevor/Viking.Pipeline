using System;

namespace Viking.Pipeline
{
    public class PlugAndPlayPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public PlugAndPlayPipelineStage(string name, IPipelineStage<TValue> source)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            this.AddDependencies(source);
        }

        public string Name { get; }
        public IPipelineStage<TValue> Source { get; private set; }

        public void ChangeSource(IPipelineStage<TValue> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (Equals(Source, source))
                return;

            this.RemoveDependencies(Source);
            Source = source;
            this.AddDependencies(source);
        }

        public TValue GetValue() => Source.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);
    }
}
