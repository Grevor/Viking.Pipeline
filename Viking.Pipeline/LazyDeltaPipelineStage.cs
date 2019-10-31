using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Supports lazy delta extraction. The delta and baseline is only recalculated when someone requests the value. 
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The delta type.</typeparam>
    public sealed class LazyDeltaPipelineStage<TInput, TOutput> : IPipelineStage<TOutput>
    {
        /// <summary>
        /// Creates a new <see cref="LazyDeltaPipelineStage{TInput, TOutput}"/> with the specified name, extractor and input.
        /// </summary>        
        /// <param name="name">The name of this stage.</param>
        /// <param name="extractor">The function used to extract a delta.</param>
        /// <param name="input">The pipeline stage which to extract delta from.</param>
        public LazyDeltaPipelineStage(
            string name,
            DeltaExtractor<TInput, TOutput> extractor,
            IPipelineStage<TInput> input)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            OldValue = input.GetValue();
            CurrentDelta = default!;
            DeltaIsValid = false;
            this.AddDependencies(input);
        }

        public string Name { get; }

        private DeltaExtractor<TInput, TOutput> Extractor { get; }
        private IPipelineStage<TInput> Input { get; }

        private bool DeltaIsValid { get; set; }
        private TInput OldValue { get; set; }
        private TOutput CurrentDelta { get; set; }

        public TOutput GetValue()
        {
            RecalculateDeltaIfNeeded();
            return CurrentDelta;
        }

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            DeltaIsValid = false;
            invalidator.InvalidateAllDependentStages(this);
        }

        private void RecalculateDeltaIfNeeded()
        {
            if (DeltaIsValid)
                return;

            var newValue = Input.GetValue();
            CurrentDelta = Extractor(OldValue, newValue);
            OldValue = newValue;
            DeltaIsValid = true;
        }
    }
}
