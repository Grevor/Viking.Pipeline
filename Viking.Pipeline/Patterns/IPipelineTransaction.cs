using System;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// The result of a <see cref="IPipelineTransaction"/> commit.
    /// </summary>
    public enum PipelineTransactionCommitResult
    {
        /// <summary>
        /// The commit was sucessful.
        /// </summary>
        Success,
        /// <summary>
        /// The commit is pending completion.
        /// </summary>
        Pending,
        /// <summary>
        /// The commit failed.
        /// </summary>
        Failed
    }

    public delegate bool PipelineUpdateAction();
    public interface IPipelineTransaction
    {
        IPipelineTransaction Update(IPipelineStage stage, PipelineUpdateAction update);
        PipelineTransactionCommitResult Commit();
    }

    public static class PipelineUpdateExtensions
    {
        /// <summary>
        /// Gets an update action denoting no action and always returning true.
        /// </summary>
        public static PipelineUpdateAction NoAction { get; } = NoUpdateAction;

        /// <summary>
        /// Sets the value of the specified <see cref="AssignablePipelineStage{TValue}"/>, and adds it to the atomic update.
        /// </summary>
        /// <typeparam name="TValue">The data type.</typeparam>
        /// <param name="stage">The stage to set value for.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The same update object.</returns>
        public static IPipelineTransaction Update<TValue>(this IPipelineTransaction transaction, AssignablePipelineStage<TValue> stage, TValue value)
        {
            bool AssignableUpdate() => stage.SetValueWithoutInvalidating(value);

            return transaction.Update(stage, AssignableUpdate);
        }

        /// <summary>
        /// Adds the specified stage to the update, without doing any updating.
        /// </summary>
        /// <param name="stage">The stage to add for the update.</param>
        /// <returns>The same update object.</returns>
        public static IPipelineTransaction Update(this IPipelineTransaction transaction, IPipelineStage stage) => transaction.Update(stage, NoAction);

        private static bool NoUpdateAction() => true;
    }
}