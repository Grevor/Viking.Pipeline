using System.Collections.Generic;

namespace Viking.Pipeline
{
    /// <summary>
    /// Provides functionality to invalidate, revalidate and check the current status of a pipeline propagation.
    /// </summary>
    public interface IPipelineInvalidator
    {
        /// <summary>
        /// Invalidates the specified stage.
        /// </summary>
        /// <param name="stage">The stage to invalidate.</param>
        /// <returns>True if the stage was not already invalidated.</returns>
        bool Invalidate(IPipelineStage stage);
        /// <summary>
        /// Invalidates all stages with dependencies to the specified stage.
        /// </summary>
        /// <param name="stage">The stage in question.</param>
        void InvalidateAllDependentStages(IPipelineStage stage);

        /// <summary>
        /// Revalidate the specified stage.
        /// </summary>
        /// <param name="stage">The stage to revalidate.</param>
        /// <returns>True of if the stage was invalid and is now revalidated.</returns>
        bool Revalidate(IPipelineStage stage);

        /// <summary>
        /// Checks if the specified stage is invalidated in this pipeline propagation.
        /// </summary>
        /// <param name="stage">The stage to check.</param>
        /// <returns>True if the specified stage is invalidated.</returns>
        bool IsInvalidated(IPipelineStage stage);

        /// <summary>
        /// Gets all invalidated stages (thus far).
        /// </summary>
        IEnumerable<IPipelineStage> AllInvalidatedStages { get; }
    }
}
