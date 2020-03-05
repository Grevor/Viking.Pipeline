using System;
using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline
{
    internal class PipelineAnalyzer
    {
        private enum Mark
        {
            NoMark,
            TemporaryMark,
            PermanentMark
        }

        public PipelineAnalyzer(Dictionary<AmbivalentPipelineReference, List<WeakReference<IPipelineStage>>> dependentStages)
        {
            DependentStages = dependentStages ?? throw new ArgumentNullException(nameof(dependentStages));
        }

        private Dictionary<AmbivalentPipelineReference, List<WeakReference<IPipelineStage>>> DependentStages { get; }
        private Dictionary<IPipelineStage, Mark> Marks { get; } = new Dictionary<IPipelineStage, Mark>();
        private Dictionary<IPipelineStage, TarjanData> Tarjan { get; } = new Dictionary<IPipelineStage, TarjanData>();

        private IEnumerable<WeakReference<IPipelineStage>> GetDependentStages(IPipelineStage stage)
            => DependentStages.TryGetValue(new AmbivalentPipelineReference(stage, true), out var deps) ? deps : Enumerable.Empty<WeakReference<IPipelineStage>>();

        #region Topology Sort
        private Mark GetMark(IPipelineStage stage) => Marks.TryGetValue(stage, out var mark) ? mark : Mark.NoMark;
        private Mark SetMark(IPipelineStage stage, Mark mark) => Marks[stage] = mark;


        public List<PipelineStagePropagation>? GetTopologySorted(IEnumerable<IPipelineStage> initial)
        {
            Marks.Clear();
            var result = new List<PipelineStagePropagation>();

            foreach (var n in initial)
                if (Visit(n, result))
                    return null;

            result.Reverse();
            return result;
        }

        private bool Visit(IPipelineStage stage, List<PipelineStagePropagation> result)
        {
            var mark = GetMark(stage);
            if (mark == Mark.TemporaryMark) return true;
            if (mark == Mark.PermanentMark) return false;

            SetMark(stage, Mark.TemporaryMark);
            var dependents = new List<IPipelineStage>();
            foreach (var wr in GetDependentStages(stage))
            {
                if (!wr.TryGetTarget(out var dependent))
                    continue;
                dependents.Add(dependent);
                if (Visit(dependent, result))
                    return true;
            }

            SetMark(stage, Mark.PermanentMark);
            result.Add(new PipelineStagePropagation(stage, dependents));
            return false;
        }
        #endregion

        #region Cycle Finder

        private TarjanData GetTarjan(IPipelineStage stage)
        {
            if (!Tarjan.TryGetValue(stage, out var tarjan))
            {
                tarjan = new TarjanData(stage);
                Tarjan.Add(stage, tarjan);
            }
            return tarjan;
        }

        public IEnumerable<IEnumerable<IPipelineStage>> FindCycles(IEnumerable<IPipelineStage> initial)
        {
            Tarjan.Clear();
            var S = new Stack<TarjanData>();
            var index = 1;
            var result = new List<List<IPipelineStage>>();
            foreach (var n in initial)
            {
                var v = GetTarjan(n);
                if (v.Index == TarjanData.Undefined)
                    StrongConnect(S, v, ref index, result);
            }
            return result;
        }

        private void StrongConnect(Stack<TarjanData> s, TarjanData v, ref int index, List<List<IPipelineStage>> output)
        {
            v.Index = index;
            v.LowLink = index++;
            s.Push(v);
            v.OnStack = true;
            foreach (var n in GetDependentStages(v.Stage))
            {
                if (!n.TryGetTarget(out var dependent))
                    continue;

                var w = GetTarjan(dependent);
                if (w.Index == TarjanData.Undefined)
                {
                    StrongConnect(s, w, ref index, output);
                    v.LowLink = Math.Min(v.LowLink, w.LowLink);
                }
                else if (w.OnStack)
                {
                    v.LowLink = Math.Min(v.LowLink, w.Index);
                }
            }

            if (v.LowLink == v.Index)
            {
                var ssc = new List<IPipelineStage>();
                TarjanData w;
                do
                {
                    w = s.Pop();
                    w.OnStack = false;
                    ssc.Add(w.Stage);
                } while (w != v);

                output.Add(ssc);
            }
        }

        private class TarjanData
        {
            public const int Undefined = -1;
            public TarjanData(IPipelineStage stage)
            {
                Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            }

            public IPipelineStage Stage { get; }
            public int Index { get; set; } = Undefined;
            public int LowLink { get; set; } = Undefined;
            public bool OnStack { get; set; } = false;
        }

        #endregion
    }
}
