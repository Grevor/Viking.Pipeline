using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    internal struct PipelineStagePropagation
    {
        public PipelineStagePropagation(IPipelineStage stage, List<IPipelineStage> dependent)
        {
            Stage = stage;
            Dependent = dependent;
        }

        public IPipelineStage Stage { get; }
        public List<IPipelineStage> Dependent { get; }
    }

    internal class PipelinePropagation
    {
        public PipelineAnalyzer Analyzer { get; private set; }
        public PipelineErrorHandler ErrorHandler { get; private set; }

        public List<PipelineStagePropagation>? CurrentPropagationTopology { get; private set; }

        public HashSet<IPipelineStage> StagesThisPropagation { get; } = new HashSet<IPipelineStage>();

        public HashSet<IPipelineStage> StagesToIgnore { get; } = new HashSet<IPipelineStage>();

        public long PipelineVersionOfCurrentPropagation { get; private set; }


        public PipelinePropagation(IEnumerable<IPipelineStage> initialStages, Dictionary<AmbivalentReference<IPipelineStage>, List<WeakReference<IPipelineStage>>> dependent)
        {
            Analyzer = new PipelineAnalyzer(dependent);
            ErrorHandler = new PipelineErrorHandler(initialStages);
        }

        public void BuildPropagationTopology(IEnumerable<IPipelineStage> stages, long pipelineVersion)
        {
            PipelineVersionOfCurrentPropagation = pipelineVersion;
            CurrentPropagationTopology = Analyzer.GetTopologySorted(stages);

            if (CurrentPropagationTopology == null)
                throw ErrorHandler.CreatePipelineCycleException(Analyzer.FindCycles(stages));

            CurrentPropagationTopology = CurrentPropagationTopology.Where(stage => !StagesToIgnore.Contains(stage.Stage)).ToList();

            StagesThisPropagation.Clear();
            foreach (var stage in CurrentPropagationTopology.Select(s => s.Stage))
                StagesThisPropagation.Add(stage);
        }

        public void PrepareForPotentialNewPropagation()
        {
            StagesToIgnore.UnionWith(StagesThisPropagation);
            StagesThisPropagation.Clear();
        }

        public void Propagate(IEnumerable<IPipelineStage> stages)
        {
            if (CurrentPropagationTopology == null)
                throw new InvalidOperationException("Topology has not been successfully created.");

            var invalidator = new PipelineInvalidator();
            foreach (var initialStages in stages)
                invalidator.Invalidate(initialStages);

            try
            {
                for(int stageIndex = 0; stageIndex < CurrentPropagationTopology.Count;++stageIndex)
                {
                    var stage = CurrentPropagationTopology[stageIndex];
                    CurrentPropagationTopology[stageIndex] = default;
                    if (!invalidator.IsInvalidated(stage.Stage))
                        continue;

                    invalidator.PrepareForNextStage(stage.Dependent);
                    ErrorHandler.SetCurrentStage(stage.Stage);

                    stage.Stage.OnInvalidate(invalidator);

                    ErrorHandler.SetDependent(invalidator.InvalidatedByThisStep);
                }
            }
            catch (Exception exception)
            {
                throw ErrorHandler.CreatePipelinePropagationException(exception);
            }
        }

        public void CheckForConcurrentPropagation(HashSet<IPipelineStage> currentlyUpdatingPipelineStages)
        {
            var potentialConcurrentPropagation = new HashSet<IPipelineStage>(StagesThisPropagation);
            potentialConcurrentPropagation.IntersectWith(currentlyUpdatingPipelineStages);

            if (potentialConcurrentPropagation.Count > 0)
                throw ErrorHandler.CreatePipelineConcurrentPropagationException(potentialConcurrentPropagation);

            currentlyUpdatingPipelineStages.UnionWith(StagesThisPropagation);
        }
    }
}
