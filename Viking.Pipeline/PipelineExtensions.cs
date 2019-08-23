using System;
using System.Collections.Generic;
using System.Text;

namespace Viking.Pipeline
{
    public static partial class PipelineCore
    {
        public static CachingPipelineStage<T> WithCache<T>(this IPipelineStage<T> stage) => new CachingPipelineStage<T>(stage);
        public static DetachablePipelineStage<T> AsDetachable<T>(this IPipelineStage<T> stage) => new DetachablePipelineStage<T>(stage);

        public static IPipelineStage<T> AsPassive<T>(this IPipelineStage<T> stage) => new PassivePipelineStage<T>(stage);
        public static IPipelineStage<T> WithNewName<T>(this IPipelineStage<T> stage, string name) => new PassThroughPipelineStage<T>(name, stage);
        public static IPipelineStage<T> AsAsync<T>(this IPipelineStage<T> stage) => new AsyncPipelineStage<T>(stage);
        public static IPipelineStage<T> AsEager<T>(this IPipelineStage<T> stage) => new EagerPipelineStage<T>(stage);
        public static IPipelineStage<T> WithConditionalSuspender<T>(this IPipelineStage<T> stage, IPipelineStage<PipelineSuspension> suspender) => new SuspendingPipelineStage<T>(stage, suspender);
        public static IPipelineStage<T> ExceptWhen<T>(this IPipelineStage<T> stage, params IPipelineStage[] mutuallyExclusiveStages) => new MutuallyExclusivePipelineStage<T>(stage, mutuallyExclusiveStages);

        public static IPipelineStage<T> WithEqualityCheck<T>(this IPipelineStage<T> stage) => new EqualityCheckerPipelineStage<T>(stage);
        public static IPipelineStage<T> WithEqualityCheck<T>(this IPipelineStage<T> stage, IEqualityComparer<T> comparer) => new EqualityCheckerPipelineStage<T>(stage, comparer);
        public static IPipelineStage<T> WithEqualityCheck<T>(this IPipelineStage<T> stage, EqualityCheck<T> comparer) => new EqualityCheckerPipelineStage<T>(stage, comparer);

        public static IPipelineStage<T> AsPipelineConstant<T>(this T value) => new ConstantPipelineStage<T>(value);
        public static IPipelineStage<T> AsPipelineConstant<T>(this T value, string name) => new ConstantPipelineStage<T>(name, value);

        public static IPipelineStage CreateReaction(this IPipelineStage stage, Action reaction) => new ReactionPipelineStage(reaction, stage);
        public static IPipelineStage<T> CreateReaction<T>(this IPipelineStage<T> stage, Action<T> reaction) => new ReactionPipelineStage<T>(reaction, stage);


        public static string GetClassAndMethod(this Delegate del)
        {
            var method = del.Method;
            return FormattableString.Invariant($"{method.DeclaringType.Name}.{method.Name}");
        }
        public static string GetDetailedStringRepresentation(this Delegate del)
        {
            var builder = new StringBuilder();
            builder.Append(del.GetClassAndMethod());
            if (!del.Method.IsStatic)
                builder.Append(" on object ").Append(del.Target);
            return builder.ToString();
        }
    }
}
