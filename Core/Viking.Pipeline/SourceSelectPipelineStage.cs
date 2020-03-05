using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Enables selecting between any number of pipeline stages as output.
    /// </summary>
    /// <typeparam name="TValue">The type of output from this stage.</typeparam>
    public sealed class SourceSelectPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="SourceSelectPipelineStage{TValue}"/> with the specified name and initial source.
        /// </summary>
        /// <param name="name">The name of this stage. Must not be null.</param>
        /// <param name="source">The initial source. Must no be null.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="name"/> or <paramref name="source"/> is null.</exception>
        public SourceSelectPipelineStage(string name, IPipelineStage<TValue> source)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            this.AddDependencies(source);
        }

        public string Name { get; }

        /// <summary>
        /// Gets the currently selected source.
        /// </summary>
        public IPipelineStage<TValue> Source { get; private set; }

        /// <summary>
        /// Sets the source of this <see cref="SourceSelectPipelineStage{TValue}"/>.
        /// </summary>
        /// <param name="source">The new source. Must not be null.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="source"/> is null.</exception>
        public bool SetSource(IPipelineStage<TValue> source) => SetSource(source, true);
        /// <summary>
        /// Sets the source of this <see cref="SourceSelectPipelineStage{TValue}"/> without notifying the rest of the pipeline.
        /// </summary>
        /// <param name="source">The new source. Must not be null.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="source"/> is null.</exception>
        public bool SetSourceWithoutInvalidating(IPipelineStage<TValue> source) => SetSource(source, false);
        private bool SetSource(IPipelineStage<TValue> source, bool invalidate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (Equals(Source, source))
                return false;

            this.RemoveDependencies(Source);
            Source = source;
            this.AddDependencies(source);

            if (invalidate)
                this.Invalidate();
            return true;
        }

        public TValue GetValue() => Source.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Source Selector - Current Source: '{Source.Name}'");
    }
}
