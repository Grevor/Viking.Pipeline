using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Retrieves data from a function and makes it available to the pipeline.
    /// </summary>
    /// <typeparam name="TValue">The output value type.</typeparam>
    public sealed class DataRetrievalPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="DataRetrievalPipelineStage{TValue}"/> with the specified name and source.
        /// </summary>
        /// <param name="name">The name of this stage.</param>
        /// <param name="source">The function from which to retrieve the value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="source"/> is null.</exception>
        public DataRetrievalPipelineStage(string name, Func<TValue> source)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public string Name { get; }
        /// <summary>
        /// Gets the source from which this stage retrieves its values.
        /// </summary>
        public Func<TValue> Source { get; }

        public TValue GetValue() => Source();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Data Retrieval - Source: {Source.GetClassAndMethod()}");
    }
}
