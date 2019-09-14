using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Enables atomic update of stages through a fluid interface.
    /// </summary>
    public sealed class PipelineTransaction : IPipelineTransaction
    {
        private int Timestamp { get; set; }
        private Dictionary<IPipelineStage, DeferredTransactionPart> PendingStages { get; } = new Dictionary<IPipelineStage, DeferredTransactionPart>();

        /// <summary>
        /// Starts a new <see cref="PipelineTransaction"/>.
        /// </summary>
        public PipelineTransaction() { }

        public IPipelineTransaction Update(IPipelineStage stage, PipelineUpdateAction update)
        {
            if (stage is null)
                throw new System.ArgumentNullException(nameof(stage));
            if (update is null)
                throw new System.ArgumentNullException(nameof(update));

            PendingStages[stage] = new DeferredTransactionPart(stage, update, Timestamp++);

            return this;
        }

        /// <summary>
        /// Complete the update, invalidating all updated stages as an atomic operation.
        /// </summary>
        /// <returns><see cref="PipelineTransactionResult.Success"/></returns>
        public PipelineTransactionResult Commit()
        {
            var stagesToInvalidate = PendingStages.Values.OrderBy(part => part.Timestamp).Where(part => part.Action()).Select(part => part.Stage).ToList();
            PipelineCore.Invalidate(stagesToInvalidate);
            PendingStages.Clear();

            return PipelineTransactionResult.Success;
        }

        public void Cancel() => PendingStages.Clear();
    }
}
