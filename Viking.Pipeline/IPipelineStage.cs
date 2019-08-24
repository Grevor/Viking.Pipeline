namespace Viking.Pipeline
{
    public interface IPipelineStage
    {
        /// <summary>
        /// The name of this <see cref="IPipelineStage"/>.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Defines the behavior of a pipeline stage after it (and potential dependencies) has been invalidated.
        /// </summary>
        /// <param name="invalidator">The invalidator, holding the current state of the pipeline propagation.</param>
        void OnInvalidate(IPipelineInvalidator invalidator);
    }

    public interface IPipelineStage<TOutput> : IPipelineStage
    {
        /// <summary>
        /// Gets the value which is the output of this <see cref="IPipelineStage{TOutput}"/>.
        /// </summary>
        /// <returns>The output value of this stage.</returns>
        TOutput GetValue();
    }
}
