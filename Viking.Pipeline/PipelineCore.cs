using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    /// <summary>
    /// The core of the pipeline. Contains core functionality like add/remove dependencies and invalidate.
    /// </summary>
    public static partial class PipelineCore
    {
        private const int MinimumOperationsRequiredBeforeCleanup = 100;

        #region Lock-protected Core Properties
        private static HashSet<IPipelineStage> PotentialStagesForUpdate { get; } = new HashSet<IPipelineStage>();
        private static Dictionary<WeakHashKey<IPipelineStage>, List<WeakReference<IPipelineStage>>> Dependencies { get; }
            = new Dictionary<WeakHashKey<IPipelineStage>, List<WeakReference<IPipelineStage>>>();
        private static int OperationsSinceLastCleanup { get; set; }
        private static int TotalWeakKeys { get; set; }
        private static long PipelineVersion { get; set; }
        #endregion

        private static void IncrementOperation()
        {
            var ops = ++OperationsSinceLastCleanup;
            var num = TotalWeakKeys;
            num *= num;

            if (ops >= num && ops > MinimumOperationsRequiredBeforeCleanup)
                CleanUp();
        }

        private static int CleanUp()
        {
            var completeRemovals = new List<WeakHashKey<IPipelineStage>>();
            var keys = TotalWeakKeys;
            foreach(var entry in Dependencies)
            {
                if (entry.Key.IsAlive)
                    keys -= RemoveNonExisting(entry.Value);
                else
                {
                    completeRemovals.Add(entry.Key);
                    keys -= entry.Value.Count;
                }
            }
            foreach (var keyToRemove in completeRemovals)
                Dependencies.Remove(keyToRemove);
            OperationsSinceLastCleanup = 0;

            var removals = TotalWeakKeys - keys;
            TotalWeakKeys = keys;
            return removals;
        }
        private static void MarkPipelineAsUpdated() => ++PipelineVersion;

        /// <summary>
        /// Cleans up the pipeline and any lingering metadata objects directly.
        /// </summary>
        public static int CleanUpPending()
        {
            lock (Dependencies)
                return CleanUp();
        }

        /// <summary>
        /// Gets the minimal pipeline graph where all the specified stages are included.
        /// </summary>
        /// <param name="stages">The stages.</param>
        /// <returns>The pipeline graph. In case of errors, this might be invalid.</returns>
        public static PipelineGraph GetPipelineGraphIncludingStages(IEnumerable<IPipelineStage> stages)
        {
            var graph = new PipelineGraph();
            try
            {
                var propagation = new PipelinePropagation(stages, Dependencies);
                lock (Dependencies)
                    propagation.BuildPropagationTopology(stages, 0);

                foreach (var s in propagation.CurrentPropagationTopology)
                {
                    graph.AddNode(s.Stage);
                    foreach (var dep in s.Dependent)
                        graph.AddEdge(s.Stage, dep);
                }

                return graph;
            }
            catch
            {
                graph.Invalidate();
                return graph;
            }
        }

        /// <summary>
        /// Invalidates this pipeline stage, propagating it through the pipeline.
        /// </summary>
        /// <param name="stage">The stage top invalidate.</param>
        public static void Invalidate(this IPipelineStage stage) => Invalidate(new[] { stage });
        /// <summary>
        /// Invalidates all specified pipeline stages.
        /// </summary>
        /// <param name="stages">The stages to invalidate.</param>
        public static void Invalidate(this IEnumerable<IPipelineStage> stages) => InvalidatePipeline(stages);
        /// <summary>
        /// Invalidates all specified pipeline stages.
        /// </summary>
        /// <param name="stages">The stages to invalidate.</param>
        public static void Invalidate(params IPipelineStage[] stages) => InvalidatePipeline(stages);
        private static void InvalidatePipeline(IEnumerable<IPipelineStage> stages)
        {
            var propagation = new PipelinePropagation(stages, Dependencies);
            lock (Dependencies)
            {
                propagation.BuildPropagationTopology(stages, PipelineVersion);
                propagation.CheckForConcurrentPropagation(PotentialStagesForUpdate);
            }

            try
            {
                propagation.Propagate(stages);
            }
            finally
            {
                lock (Dependencies)
                {
                    PotentialStagesForUpdate.ExceptWith(propagation.StagesThisPropagation);
                    IncrementOperation();
                }
            }
        }

        /// <summary>
        /// Add dependencies for this pipeline stage, propagation on their invalidation.
        /// </summary>
        /// <param name="dependee">The stage to add dependencies for.</param>
        /// <param name="dependencies">The dependencies to add.</param>
        public static void AddDependencies(this IPipelineStage dependee, params IPipelineStage[] dependencies)
        {
            lock (Dependencies)
            {
                foreach (var dependency in dependencies)
                {
                    var weak = new WeakHashKey<IPipelineStage>(dependency, false);
                    if (!Dependencies.TryGetValue(weak, out var deps))
                    {
                        deps = new List<WeakReference<IPipelineStage>>();
                        Dependencies.Add(weak, deps);
                    }
                    deps.Add(new WeakReference<IPipelineStage>(dependee));
                    ++TotalWeakKeys;

                    GC.KeepAlive(dependency); //keep the reference alive at least until it can be added in the dictionary
                }
                IncrementOperation();
                MarkPipelineAsUpdated();
            }
        }
        /// <summary>
        /// Removes the specified dependencies from this stage.
        /// </summary>
        /// <param name="dependee">The stage to remove dependencies for.</param>
        /// <param name="dependencies">The dependencies to remove.</param>
        public static void RemoveDependencies(this IPipelineStage dependee, params IPipelineStage[] dependencies)
        {
            lock (Dependencies)
            {
                foreach (var dependency in dependencies)
                {
                    var strong = new WeakHashKey<IPipelineStage>(dependency, true);
                    if (!Dependencies.TryGetValue(strong, out var deps))
                        continue;
                    TotalWeakKeys -= deps.RemoveAll(r => !r.TryGetTarget(out var reference) || reference == dependee);

                    GC.KeepAlive(dependency);
                }
                IncrementOperation();
                // TODO: remove this? Removed dependencies should never affect the propagation of the pipeline graph in any way.
                MarkPipelineAsUpdated();
            }
        }

        /// <summary>
        /// Get all stages which depend in this stage.
        /// </summary>
        /// <param name="stage">The stage to get dependencies for.</param>
        /// <returns>The dependent stages.</returns>
        public static IEnumerable<IPipelineStage> GetAllDependentStages(this IPipelineStage stage) =>
            InternalGetDependencies(stage)
            .Select(s => s.TryGetTarget(out var target) ? target : null)
            .Where(s => s != null)
            .ToList();



        private static IEnumerable<WeakReference<IPipelineStage>> InternalGetDependencies(IPipelineStage stage)
        {
            IncrementOperation();
            var strong = new WeakHashKey<IPipelineStage>(stage, true);
            if (!Dependencies.TryGetValue(strong, out var dependencies))
                return Enumerable.Empty<WeakReference<IPipelineStage>>();

            TotalWeakKeys -= RemoveNonExisting(dependencies);

            return dependencies;
        }

        private static int RemoveNonExisting(List<WeakReference<IPipelineStage>> dependencies)
        {
            var compact = 0;
            for(int i = 0; i < dependencies.Count; ++i)
            {
                var item = dependencies[i];
                if (item.TryGetTarget(out var _))
                {
                    dependencies[compact++] = dependencies[i];
                }
            }

            var removals = dependencies.Count - compact;
            dependencies.RemoveRange(compact, removals);
            return removals;
        }

        internal static void UnlinkAllDependencies(this IPipelineStage stage)
        {
            lock (Dependencies)
            {
                IncrementOperation();
                Dependencies.Remove(new WeakHashKey<IPipelineStage>(stage, true));
                MarkPipelineAsUpdated();
            }
        }
    }
}
