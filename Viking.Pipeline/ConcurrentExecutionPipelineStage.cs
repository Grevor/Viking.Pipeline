using System;
using System.Threading.Tasks;

namespace Viking.Pipeline
{
    /// <summary>
    /// Enables concurrent execution of a pipeline stage.
    /// </summary>
    /// <typeparam name="TOutput">The output.</typeparam>
    public class ConcurrentExecutionPipelineStage<TOutput> : IPipelineStage<Task<TOutput>>
    {
        /// <summary>
        /// Creates a new <see cref="ConcurrentExecutionPipelineStage{TOutput}"/> for the specified input.
        /// </summary>
        /// <param name="input">The stage to execute concurrently.</param>
        public ConcurrentExecutionPipelineStage(IPipelineStage<TOutput> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Async task for: " + Input.Name;
            this.AddDependencies(Input);
        }

        public IPipelineStage<TOutput> Input { get; }
        public string Name { get; }

        public Task<TOutput> GetValue() => Task.Run(Input.GetValue);

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);
    }
}
