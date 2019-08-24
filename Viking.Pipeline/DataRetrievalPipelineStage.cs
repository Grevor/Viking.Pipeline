using System;

namespace Viking.Pipeline
{
    public class DataRetrievalPipelineStage<TValue> : IPipelineStage<TValue>
    {
        public DataRetrievalPipelineStage(string name, Func<TValue> source)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public string Name { get; }
        public Func<TValue> Source { get; }

        public TValue GetValue() => Source();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Data Retrieval - Source: {Source.GetClassAndMethod()}");
    }
}
