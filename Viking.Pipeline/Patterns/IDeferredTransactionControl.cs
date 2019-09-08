using System.Collections.Generic;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Interface of an object controlling transactions.
    /// </summary>
    public interface IDeferredTransactionControl
    {
        /// <summary>
        /// Gets a timestamp, denoting some sort of time relevant to the execution of transactions.
        /// </summary>
        /// <returns></returns>
        long GetTimestamp();

        /// <summary>
        /// Registers the specified transaction to this control.
        /// </summary>
        /// <param name="transaction">The transaction to register.</param>
        void Register(DeferredPipelineTransaction transaction);
        /// <summary>
        /// Commits the specified transaction to the control.
        /// </summary>
        /// <param name="transaction">The transaction to commit.</param>
        /// <param name="parts">The parts.</param>
        /// <returns>The result of the commit.</returns>
        PipelineTransactionResult Commit(DeferredPipelineTransaction transaction, IEnumerable<DeferredTransactionPart> parts);

        /// <summary>
        /// Cancels the specified transaction from this control.
        /// </summary>
        /// <param name="transaction">The transaction to cancel.</param>
        void Cancel(DeferredPipelineTransaction transaction);
    }
}
