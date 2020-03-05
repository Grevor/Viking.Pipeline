using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viking.Pipeline
{
    internal class PipelineErrorHandler
    {
        public PipelineErrorHandler(IEnumerable<IPipelineStage> initialStages)
        {
            InitialStages = initialStages ?? throw new ArgumentNullException(nameof(initialStages));
        }

        private IEnumerable<IPipelineStage> InitialStages { get; }
        private IPipelineStage CurrentStage => ExecutionOrder[ExecutionOrder.Count - 1];
        private List<IPipelineStage> ExecutionOrder { get; } = new List<IPipelineStage>();
        private Dictionary<IPipelineStage, List<IPipelineStage>> ActualExecution { get; } = new Dictionary<IPipelineStage, List<IPipelineStage>>();

        public void SetCurrentStage(IPipelineStage stage) => ExecutionOrder.Add(stage);
        public void SetDependent(List<IPipelineStage> dependents) => ActualExecution.Add(CurrentStage, dependents);

        private static string InitialHeader => "Exception while propagating pipeline update";


        public Exception CreatePipelinePropagationException(Exception ex)
        {
            var builder = new StringBuilder(1000);
            builder
                .AppendLargeHeader(InitialHeader)
                .Append("Pipeline Stage Throwing Exception: ").AppendLine(CurrentStage.GetErrorInfo());
            AppendInitialStages(builder);
            builder
                .AppendSmallHeader("Exception")
                .AppendLine(ex.Message)
                .AppendLine();
            AppendPipelineTrace(builder);

            return new PipelineException(builder.ToString(), ex);
        }

        public Exception CreatePipelineCycleException(IEnumerable<IEnumerable<IPipelineStage>> ssc)
        {
            var builder = new StringBuilder(1000);
            builder
                .AppendLargeHeader(InitialHeader)
                .Append("Found potential cycle(s) in pipeline");
            AppendInitialStages(builder);

            builder.AppendLargeHeader("Potential components where cycles can happen");
            foreach (var sscc in ssc)
            {
                builder.AppendLine(string.Join(Environment.NewLine + "\t", sscc.Select(s => s.GetErrorInfo())));
                builder.AppendLine();
            }
            return new PipelineException(builder.ToString());
        }

        public Exception CreatePipelineConcurrentPropagationException(IEnumerable<AmbivalentPipelineReference> potentialOverlaps)
        {
            var builder = new StringBuilder(1000);
            builder
                .AppendLargeHeader(InitialHeader)
                .AppendLine("Found potential concurrent propagation through one or more stages");
            AppendInitialStages(builder);

            builder.AppendSmallHeader("Potential stages with concurrent propagation");
            foreach (var potential in potentialOverlaps.Select(reference => reference.TryGetTarget(out var target) ? target : null).Where(target => target != null))
                builder.AppendLine(potential!.GetErrorInfo());

            return new PipelineException(builder.ToString());
        }


        private void AppendInitialStages(StringBuilder builder)
        {
            if (InitialStages.Count() == 1)
                builder.Append("Initial stage: ").AppendLine(InitialStages.First().GetErrorInfo()).AppendLine();
            else
                builder
                    .AppendLine("Initial stages:")
                    .AppendLine(string.Join(Environment.NewLine, InitialStages.Select(s => s.GetErrorInfo())))
                    .AppendLine();
        }
        private void AppendPipelineTrace(StringBuilder builder)
        {
            builder.AppendLargeHeader("Full Pipeline Trace");
            foreach (var stage in ExecutionOrder)
                AppendTrace(builder, stage);
        }
        private void AppendTrace(StringBuilder builder, IPipelineStage stage)
        {
            builder.AppendSmallHeader(stage.Name)
                .AppendLine(string.Join(Environment.NewLine + "\t-> ", ActualExecution[stage].Select(s => s.GetErrorInfo())))
                .AppendLine();
        }
    }

    internal static class StringBuilderExtensions
    {
        private static string LargeHeader { get; } = new string('#', 60);
        public static StringBuilder AppendLargeHeader(this StringBuilder builder, string header)
        {
            return builder
                .AppendLine(LargeHeader)
                .Append("## ").AppendLine(header)
                .AppendLine(LargeHeader);
        }

        public static StringBuilder AppendSmallHeader(this StringBuilder builder, string header) => builder.Append("# ").Append(header).AppendLine(" #");
        public static StringBuilder AppendIndentedLine(this StringBuilder builder, string line, int indent) => builder.Append('\t', indent).AppendLine(line);
        public static string GetErrorInfo(this IPipelineStage stage) => FormattableString.Invariant($"{stage.Name}{new string(' ', Math.Max(50 - stage.Name.Length, 1))}({stage.ToString()})");
    }
}
