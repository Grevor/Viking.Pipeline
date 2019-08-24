using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Provides caching of values in the pipeline to prevent multiple retrievals from upstream.
    /// </summary>
    /// <typeparam name="TValue">The type of value to cache.</typeparam>
    public sealed class CachingPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="CachingPipelineStage{TValue}"/> with the specified input.
        /// </summary>
        /// <param name="input">The input from which to cache values.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is null.</exception>
        public CachingPipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Cache for: " + input.Name;
            this.AddDependencies(input);
        }

        public string Name { get; }
        /// <summary>
        /// Gets the stage which to cache for.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }
        /// <summary>
        /// Checks if the cache is currently holding a valid value.
        /// </summary>
        public bool IsValid { get; private set; }
        private TValue Cached { get; set; }

        public TValue GetValue()
        {
            if (!IsValid)
            {
                Cached = Input.GetValue();
                IsValid = true;
            }
            return Cached;
        }

        /// <summary>
        /// Invalidates the cache.
        /// </summary>
        public void InvalidateCache()
        {
            if (!IsValid)
                return;
            IsValid = false;
            this.Invalidate();
        }

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            IsValid = false;
            invalidator.InvalidateAllDependentStages(this);
        }

        public override string ToString() => FormattableString.Invariant($"Cache - Valid: {IsValid} Value: {Cached}");
    }
}
