namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// The result of a <see cref="IPipelineTransaction"/> commit.
    /// </summary>
    public enum PipelineTransactionResult
    {
        /// <summary>
        /// The commit was sucessful.
        /// </summary>
        Success,
        /// <summary>
        /// The commit succeeded, but is pending completion.
        /// </summary>
        PendingSuccess,
        /// <summary>
        /// The commit failed.
        /// </summary>
        Failed
    }

    /// <summary>
    /// Action which update a pipeline stage.
    /// </summary>
    /// <returns>True if the stage was actually updated, else false.</returns>
    public delegate bool PipelineUpdateAction();

    /// <summary>
    /// Interface of a pipeline transaction.
    /// Note that a transaction does not have to abide by all the ACID rules.
    /// </summary>
    public interface IPipelineTransaction
    {
        /// <summary>
        /// Updates the specified stage using the specified update function.
        /// </summary>
        /// <param name="stage">The stage to update.</param>
        /// <param name="update">The action updating the stage.</param>
        /// <returns></returns>
        IPipelineTransaction Update(IPipelineStage stage, PipelineUpdateAction update);
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <returns>The result of the commit.</returns>
        PipelineTransactionResult Commit();
        /// <summary>
        /// Cancels the transaction, discarding all changes.
        /// </summary>
        void Cancel();
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
        /// <param name="transaction">The transaction.</param>
        /// <param name="stage">The stage to set value for.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The same update object.</returns>
        public static IPipelineTransaction Update<TValue>(this IPipelineTransaction transaction, AssignablePipelineStage<TValue> stage, TValue value)
        {
            bool AssignableUpdate() => stage.SetValueWithoutInvalidating(value);
            return transaction.Update(stage, AssignableUpdate);
        }

        public static IPipelineTransaction Update<TValue>(this IPipelineTransaction transaction, SourceSelectPipelineStage<TValue> stage, IPipelineStage<TValue> newSource)
        {
            bool SourceSelectUpdate() => stage.SetSourceWithoutInvalidating(newSource);
            return transaction.Update(stage, SourceSelectUpdate);
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