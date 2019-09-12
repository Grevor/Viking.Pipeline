using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Viking.Pipeline
{
    /// <summary>
    /// Contains extension methods for pipeline objects.
    /// </summary>
    public static partial class PipelineCore
    {
        /// <summary>
        /// Adds a <see cref="CachingPipelineStage{TValue}"/> after this stage, caching the output of this stage until invalidated.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to cache value for.</param>
        /// <returns>The caching pipeline stage.</returns>
        public static CachingPipelineStage<T> WithCache<T>(this IPipelineStage<T> stage) => new CachingPipelineStage<T>(stage);
        /// <summary>
        /// Adds a <see cref="DetachablePipelineStage{TValue}"/> after this stage, enabling quick detachment of all dependent stages from the pipeline.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to get value from.</param>
        /// <returns>The detachable stage.</returns>
        public static DetachablePipelineStage<T> AsDetachable<T>(this IPipelineStage<T> stage) => new DetachablePipelineStage<T>(stage);

        /// <summary>
        /// Adds a <see cref="PassivePipelineStage{TValue}"/> after this stage, causing no invalidations to propagate any further.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to make passive.</param>
        /// <returns>The passive pass-through pipeline stage.</returns>
        public static IPipelineStage<T> AsPassive<T>(this IPipelineStage<T> stage) => new PassivePipelineStage<T>(stage);
        /// <summary>
        /// Adds a pass-through stage after this stage with the specified name.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to change name for.</param>
        /// <param name="name">The new name.</param>
        /// <returns>A pipeline stage with the new name.</returns>
        public static IPipelineStage<T> WithNewName<T>(this IPipelineStage<T> stage, string name) => new PassThroughPipelineStage<T>(name, stage);
        /// <summary>
        /// Adds a stage which will perform the retrieval of the value from this stage using async-await.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to get asynchronously from.</param>
        /// <returns>The pipeline stage, but with async-await properties.</returns>
        public static IPipelineStage<T> AsAsync<T>(this IPipelineStage<T> stage) => new AsyncPipelineStage<T>(stage);
        /// <summary>
        /// Adds a stage which will eagerly evaluate the value of this stage when invalidated.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to make eager.</param>
        /// <returns>The eager stage.</returns>
        public static IPipelineStage<T> AsEager<T>(this IPipelineStage<T> stage) => new EagerPipelineStage<T>(stage);
        /// <summary>
        /// Adds a stage which will add thread safety to this stage's <see cref="IPipelineStage{TOutput}.GetValue"/> method.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to make thread-safe.</param>
        /// <returns>The thread-safe stage.</returns>
        public static IPipelineStage<T> AsThreadSafe<T>(this IPipelineStage<T> stage) => new ThreadSafePipelineStage<T>(stage);
        /// <summary>
        /// Adds a stage which will add thread safety to this stage's <see cref="IPipelineStage{TOutput}.GetValue"/> method.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to make thread-safe.</param>
        /// <returns>The thread-safe stage.</returns>
        public static IPipelineStage<Task<T>> WithConcurrentExecution<T>(this IPipelineStage<T> stage) => new ConcurrentExecutionPipelineStage<T>(stage);
        /// <summary>
        /// Adds a conditional suspender to this stage, only letting invalidations through if the given pipeline stage is not suspended.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to contidionaly add a suspender to.</param>
        /// <param name="suspender">The suspend state.</param>
        /// <returns>The conditionally invalidated stage.</returns>
        public static IPipelineStage<T> WithConditionalSuspender<T>(this IPipelineStage<T> stage, IPipelineStage<PipelineSuspensionState> suspender) => new SuspendingPipelineStage<T>(stage, suspender);
        /// <summary>
        /// Adds a stage which will only propagate invalidations if none of the supplied stages are invalidated.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="stage">The stage to add conditional propagation to.</param>
        /// <param name="mutuallyExclusiveStages">The mutually exclusive stages.</param>
        /// <returns>The stage which will only invalidate if none of the specified stages are invalidated.</returns>
        public static IPipelineStage<T> ExceptWhen<T>(this IPipelineStage<T> stage, params IPipelineStage[] mutuallyExclusiveStages) => new MutuallyExclusivePipelineStage<T>(stage, mutuallyExclusiveStages);
        /// <summary>
        /// Casts this stage's value to the specified type.
        /// </summary>
        /// <typeparam name="TIn">The input type.</typeparam>
        /// <typeparam name="TOut">The output type.</typeparam>
        /// <param name="stage">The stage to cast from.</param>
        /// <returns>The stage containing the cast value.</returns>
        public static IPipelineStage<TOut> Cast<TIn, TOut>(this IPipelineStage<TIn> stage) where TIn : TOut
            => PipelineOperations.Create("Cast to " + typeof(TOut).Name + " for: " + stage.Name, CastObject<TIn, TOut>, stage);
        private static TOut CastObject<TIn, TOut>(TIn input) where TIn : TOut => input;

        /// <summary>
        /// Adds an <see cref="EqualityCheckerPipelineStage{TValue}"/> after this stage.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="stage">The stage to add equality checking for.</param>
        /// <returns>The equality checking stage.</returns>
        public static IPipelineStage<T> WithEqualityCheck<T>(this IPipelineStage<T> stage) => new EqualityCheckerPipelineStage<T>(stage);
        /// <summary>
        /// Adds an <see cref="EqualityCheckerPipelineStage{TValue}"/> after this stage.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="stage">The stage to add equality checking for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing for equality.</param>
        /// <returns>The equality checking stage.</returns>
        public static IPipelineStage<T> WithEqualityCheck<T>(this IPipelineStage<T> stage, IEqualityComparer<T> comparer) => new EqualityCheckerPipelineStage<T>(stage, comparer);
        /// <summary>
        /// Adds an <see cref="EqualityCheckerPipelineStage{TValue}"/> after this stage.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="stage">The stage to add equality checking for.</param>
        /// <param name="comparer">The compare function to use when comparing for equality.</param>
        /// <returns>The equality checking stage.</returns>
        public static IPipelineStage<T> WithEqualityCheck<T>(this IPipelineStage<T> stage, EqualityCheck<T> comparer) => new EqualityCheckerPipelineStage<T>(stage, comparer);

        /// <summary>
        /// Converts this value to a <see cref="ConstantPipelineStage{TValue}"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The value to convert to a constant pipeline stage.</param>
        /// <returns>The stage containing the value as a constant.</returns>
        public static IPipelineStage<T> AsPipelineConstant<T>(this T value) => new ConstantPipelineStage<T>(value);
        /// <summary>
        /// Converts this value to a <see cref="ConstantPipelineStage{TValue}"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The value to convert to a constant pipeline stage.</param>
        /// <param name="name">The name given to the constant.</param>
        /// <returns>The stage containing the value as a constant.</returns>
        public static IPipelineStage<T> AsPipelineConstant<T>(this T value, string name) => new ConstantPipelineStage<T>(name, value);

        /// <summary>
        /// Creates a reaction to changes of this stage.
        /// </summary>
        /// <param name="stage">The stage to create a reaction for.</param>
        /// <param name="reaction">The action to perform when this stage is invalidated.</param>
        /// <returns>The <see cref="ReactionPipelineStage"/> which will react to changes as long as it is alive.</returns>
        public static IPipelineStage CreateReaction(this IPipelineStage stage, Action reaction) => new ReactionPipelineStage(reaction, stage);
        /// <summary>
        /// Creates a reaction to changes of this stage.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="stage">The stage to create a reaction for.</param>
        /// <param name="reaction">The action to perform when this stage is invalidated.</param>
        /// <returns>The <see cref="ReactionPipelineStage{TInput1}"/> which will react to changes as long as it is alive.</returns>
        public static IPipelineStage<T> CreateReaction<T>(this IPipelineStage<T> stage, Action<T> reaction) => new ReactionPipelineStage<T>(reaction, stage);
        /// <summary>
        /// Creates a reaction to changes of this stage.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="stage">The stage to create a reaction for.</param>
        /// <param name="reaction">The action to perform when this stage is invalidated.</param>
        /// <returns>The <see cref="ReactionPipelineStage{TInput1}"/> which will react to changes as long as it is alive.</returns>
        public static IPipelineStage<T> CreateReaction<T>(this IPipelineStage<T> stage, Action<T> reaction, bool reactImmediately) => new ReactionPipelineStage<T>(reaction, stage, reactImmediately);


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
