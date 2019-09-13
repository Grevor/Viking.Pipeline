using System;
using System.Threading.Tasks;

namespace Viking.Pipeline
{
    /// <summary>
    /// Retrieves upstream values in an async-await fashion.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    public sealed class AsyncPipelineStage<TValue> : IPipelineStage<TValue>
    {
        /// <summary>
        /// Creates a new <see cref="AsyncPipelineStage{TValue}"/> with the specified input.
        /// </summary>
        /// <param name="input">The stage which to retrieve value from asynchronously.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is null.</exception>
        public AsyncPipelineStage(IPipelineStage<TValue> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Async invoker for: " + input.Name;
            this.AddDependencies(input);
        }

        /// <summary>
        /// Gets the stage which will have its value retrieved asynchronously.
        /// </summary>
        public IPipelineStage<TValue> Input { get; }
        public string Name { get; }

        public TValue GetValue()
        {
            var result = new ResultCache();
            AsyncGet(result);
            return result.Value;
        }
        private async void AsyncGet(ResultCache cache) => cache.Value = await Task.Run(Input.GetValue);

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

        public override string ToString() => FormattableString.Invariant($"Async Retriever '{Input.Name}'");

        private class ResultCache
        {
            public TValue Value { get; set; }
        }
    }
}
