using System.Collections.Generic;

namespace Viking.Pipeline.Patterns
{
    /// <summary>
    /// Enables atomic update of stages through a fluid interface.
    /// </summary>
    public sealed class AtomicPipelineUpdate : IPipelineTransaction
    {
        private HashSet<IPipelineStage> PendingStages { get; } = new HashSet<IPipelineStage>();

        /// <summary>
        /// Starts a new <see cref="AtomicPipelineUpdate"/>.
        /// </summary>
        public AtomicPipelineUpdate() { }

        public IPipelineTransaction Update(IPipelineStage stage, PipelineUpdateAction update)
        {
            if (stage is null)
                throw new System.ArgumentNullException(nameof(stage));

            if (update is null)
                throw new System.ArgumentNullException(nameof(update));

            if (update.Invoke())
                PendingStages.Add(stage);

            return this;
        }

        /// <summary>
        /// Complete the update, invalidating all updated stages as an atomic operation.
        /// </summary>
        /// <returns><see cref="PipelineTransactionCommitResult.Success"/></returns>
        public PipelineTransactionCommitResult Commit()
        {
            PipelineCore.Invalidate(PendingStages);
            PendingStages.Clear();

            return PipelineTransactionCommitResult.Success;
        }
    }
}
