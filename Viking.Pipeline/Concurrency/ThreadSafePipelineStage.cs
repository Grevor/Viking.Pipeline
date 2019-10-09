using System;
using System.Threading;
using System.Threading.Tasks;

namespace Viking.Pipeline
{
    /// <summary>
    /// Makes a pipeline stage thread-safe, ensuring only one invocation of GetValue can be done at a time.
    /// In case of concurrent requests, all requests will take the result of the currently running invocation.
    /// </summary>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public class ThreadSafePipelineStage<TOutput> : IPipelineStage<TOutput>
    {
        /// <summary>
        /// Creates a new <see cref="ThreadSafePipelineStage{TOutput}"/> with the specified input to make thread-safe.
        /// </summary>
        /// <param name="input">The pipeline stage to make thread-safe.</param>
        public ThreadSafePipelineStage(IPipelineStage<TOutput> input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Name = "Thread safe: " + Input.Name;
            this.AddDependencies(Input);
        }

        /// <summary>
        /// The input to make thread-safe.
        /// </summary>
        public IPipelineStage<TOutput> Input { get; }
        public string Name { get; }

        private Task<TOutput>? _currentlyRunningTask;

        public TOutput GetValue()
        {
            var task = new Task<TOutput>(GetValueThreadSafe);
            var runningTask = Interlocked.CompareExchange(ref _currentlyRunningTask, task, null);

            if (runningTask != null)
                return WaitForRunningInvocationAndGetResultFromIt(runningTask);
            else
                return InvokeGetValueAndThenCleanUp(task);
        }

        private static TOutput WaitForRunningInvocationAndGetResultFromIt(Task<TOutput> runningTask) => runningTask.Result;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Usage", "CA2219:Do not raise exceptions in finally clauses", 
            Justification = "The exception is a fatal exception. The finally clause is needed as well.")]
        private TOutput InvokeGetValueAndThenCleanUp(Task<TOutput> task)
        {
            try
            {
                task.RunSynchronously();
                return task.Result;
            }
            finally
            {
                if (Interlocked.Exchange(ref _currentlyRunningTask, null) != task)
                    throw new InvalidOperationException("Race condition detected: The task was exchanged during the operation.");
            }
        }

        private TOutput GetValueThreadSafe() => Input.GetValue();

        public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);
    }
}
