using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viking.Pipeline
{
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
        public static IEnumerable<IPipelineStage> GetDependencies(this IPipelineStage stage)
        {
            lock (Dependencies)
                return InternalGetDependencies(stage).Select(weak => weak.TryGetTarget(out var target) ? target : null).Where(t => t != null).ToList();
        }

        public static void Invalidate(this IPipelineStage stage) => Invalidate(new[] { stage });
        public static void Invalidate(this IEnumerable<IPipelineStage> stages) => InvalidatePipeline(stages);
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
