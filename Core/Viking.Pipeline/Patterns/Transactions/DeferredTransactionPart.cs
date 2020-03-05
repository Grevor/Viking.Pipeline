using System;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Describes a part of a transaction.
    /// </summary>
    public class DeferredTransactionPart
    {
        /// <summary>
        /// Creates a new <see cref="DeferredTransactionPart"/>.
        /// </summary>
        /// <param name="stage">The stage which is updated.</param>
        /// <param name="action">The action which performs the update.</param>
        /// <param name="timestamp">The timestamp.</param>
        public DeferredTransactionPart(IPipelineStage stage, PipelineUpdateAction action, long timestamp)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Timestamp = timestamp;
        }

        /// <summary>
        /// The stage.
        /// </summary>
        public IPipelineStage Stage { get; }
        /// <summary>
        /// The update action.
        /// </summary>
        public PipelineUpdateAction Action { get; }
        /// <summary>
        /// The timestamp of the update.
        /// </summary>
        public long Timestamp { get; }
    }
}
