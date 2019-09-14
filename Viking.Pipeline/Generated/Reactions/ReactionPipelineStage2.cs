using System;
using System.Linq;

namespace Viking.Pipeline
{
    public partial class ReactionPipelineStage<TInput1, TInput2> : IPipelineStage
    {
        public ReactionPipelineStage(
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2) :
            this(GetDefaultName(input1, input2), reaction, input1, input2)
        { }

        public ReactionPipelineStage(
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2,
            bool reactImmediately) :
            this(GetDefaultName(input1, input2), reaction, input1, input2, reactImmediately)
        { }

        public ReactionPipelineStage(
            string name,
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2) :
            this(name, reaction, input1, input2, true)
        { }

        public ReactionPipelineStage(
            string name,
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2,
            bool reactImmediately)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
            Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
            Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
            this.AddDependencies(input1, input2);
            if (reactImmediately)
                Reaction(Input1.GetValue(), Input2.GetValue());
        }

        public string Name { get; }
        public Action<TInput1, TInput2> Reaction { get; }
        public IPipelineStage<TInput1> Input1 { get; }
        public IPipelineStage<TInput2> Input2 { get; }

        public void OnInvalidate(IPipelineInvalidator invalidator)
        {
            invalidator.InvalidateAllDependentStages(this);
            Reaction(Input1.GetValue(), Input2.GetValue());
        }

        public override string ToString() => $"{Name} - Reaction is {Reaction.GetDetailedStringRepresentation()}";

        private static string GetDefaultName(params IPipelineStage[] stages) => $"Reaction to {string.Join(", ", stages.Select(p => "'" + p.Name + "'"))}";
    }

    public static partial class PipelineReactions
    {
        public static IPipelineStage Create<TInput1, TInput2>(
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2)
            => new ReactionPipelineStage<TInput1, TInput2>(
                reaction,
                input1, input2);

        public static IPipelineStage Create<TInput1, TInput2>(
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2,
            bool reactImmediately)
            => new ReactionPipelineStage<TInput1, TInput2>(
                reaction,
                input1, input2,
                reactImmediately);

        public static IPipelineStage Create<TInput1, TInput2>(
            string name,
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2)
            => new ReactionPipelineStage<TInput1, TInput2>(
                name,
                reaction,
                input1, input2);

        public static IPipelineStage Create<TInput1, TInput2>(
            string name,
            Action<TInput1, TInput2> reaction,
            IPipelineStage<TInput1> input1,
            IPipelineStage<TInput2> input2,
            bool reactImmediately)
            => new ReactionPipelineStage<TInput1, TInput2>(
                name,
                reaction,
                input1, input2,
                reactImmediately);
    }
}
