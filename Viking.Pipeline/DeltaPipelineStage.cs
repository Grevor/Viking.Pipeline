using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Delegate extracting some change from an old value to a new one.
    /// </summary>
    /// <typeparam name="TInput">The data to look for changes in.</typeparam>
    /// <typeparam name="TDelta">The delta.</typeparam>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    /// <returns>The delta.</returns>
    public delegate TDelta DeltaExtractor<in TInput, out TDelta>(TInput oldValue, TInput newValue);

    /// <summary>
    /// Supports getting the delta of a pipeline stage. Eager stage.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The delta type.</typeparam>
    public class DeltaPipelineStage<TInput, TOutput> : IPipelineStage<TOutput>
    {
        /// <summary>
        /// Creates a new <see cref="DeltaPipelineStage{TInput, TOutput}"/> with the specified name, extractor and input.
        /// </summary>
        /// <param name="name">The name of this stage.</param>
        /// <param name="deltaExtractor">The function used to extract a delta.</param>
        /// <param name="input">The pipeline stage which to extract delta from.</param>
        /// <param name="initial">The initial delta value.</param>
        public DeltaPipelineStage(
            string name,
            DeltaExtractor<TInput, TOutput> deltaExtractor,
            IPipelineStage<TInput> input,
            TOutput initial)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DeltaExtractor = deltaExtractor ?? throw new ArgumentNullException(nameof(deltaExtractor));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            CurrentDelta = initial;
            OldValue = input.GetValue();
            this.AddDependencies(input);
        }

        public string Name { get; }
        /// <summary>
        /// Gets the function used to extract the delta.
        /// </summary>
        public DeltaExtractor<TInput, TOutput> DeltaExtractor { get; }
        /// <summary>
        /// Gets the pipeline stage which to extract delta for.
        /// </summary>
        public IPipelineStage<TInput> Input { get; }
        private TOutput CurrentDelta { get; set; }
        private TInput OldValue { get; set; }

        public TOutput GetValue() => CurrentDelta;

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            var newValue = Input.GetValue();
            CurrentDelta = DeltaExtractor(OldValue, newValue);
            OldValue = newValue;
            invalidator.InvalidateAllDependentStages(this);
        }
    }
}
