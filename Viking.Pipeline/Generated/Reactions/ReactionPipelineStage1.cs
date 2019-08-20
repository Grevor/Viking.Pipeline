using System;
using System.Linq;

namespace Viking.Pipeline
{
    public partial class ReactionPipelineStage<TInput1> : IPipelineStage
    {
		public ReactionPipelineStage(
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1) :
			this(GetDefaultName(input1), reaction, input1) { }

		public ReactionPipelineStage(
			string name, 
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1) :
			this(name, reaction, input1, true) { }

		public ReactionPipelineStage(
			string name, 
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1, 
			bool reactImmediately)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			this.AddDependencies(input1);
			if(reactImmediately)
				Reaction(Input1.GetValue());
		}

		public string Name { get; }
		public Action<TInput1> Reaction { get; }
		public IPipelineStage<TInput1> Input1 { get; }

		public void OnInvalidate(IPipelineInvalidator invalidator)
		{
			invalidator.InvalidateAllDependentStages(this);
			Reaction(Input1.GetValue());
		}

		public override string ToString() => $"{Name} - Reaction is {Reaction.GetDetailedStringRepresentation()}";

		private static string GetDefaultName(params IPipelineStage[] stages) => $"Reaction to {string.Join(", ", stages.Select(p=>"'" + p.Name + "'"))}";
    }

	public static partial class PipelineReactions
	{
		public static IPipelineStage Create<TInput1>(
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1)
			=> new ReactionPipelineStage<TInput1>(
				reaction,
				input1);

		public static IPipelineStage Create<TInput1>(
			string name, 
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1)
			=> new ReactionPipelineStage<TInput1>(
				name,
				reaction,
				input1);
	}
}
