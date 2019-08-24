using System;

namespace Viking.Pipeline
{
    public class SourceSelectPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public SourceSelectPipelineStage(string name, IPipelineStage<TValue> source)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            this.AddDependencies(source);
        }

        public string Name { get; }
        public IPipelineStage<TValue> Source { get; private set; }

        public void SetSource(IPipelineStage<TValue> source) => SetSource(source, true);
        public void SetSourceWithoutInvalidating(IPipelineStage<TValue> source) => SetSource(source, false);
        private void SetSource(IPipelineStage<TValue> source, bool invalidate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (Equals(Source, source))
                return;

            this.RemoveDependencies(Source);
            Source = source;
            this.AddDependencies(source);

            if (invalidate)
                this.Invalidate();
        }

        public TValue GetValue() => Source.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Source Selector - Current Source: '{Source.Name}'");
    }
}
