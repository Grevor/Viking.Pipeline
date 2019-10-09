using System;
using System.Linq;
using System.Windows.Threading;

namespace Viking.Pipeline.Wpf
{
    public partial class DispatcherReactionPipelineStage<TInput1> : IPipelineStage
    {
		public DispatcherReactionPipelineStage(
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1) :
			this(GetDefaultName(input1), reaction, input1) 
		{ }

		public DispatcherReactionPipelineStage(
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1,
			bool reactImmediately) :
			this(GetDefaultName(input1), reaction, DispatcherUtilities.DefaultDispatcher, input1, reactImmediately) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1) :
			this(name, reaction, DispatcherUtilities.DefaultDispatcher, input1, true) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1> reaction, 
			Dispatcher dispatcher,
			IPipelineStage<TInput1> input1, 
			bool reactImmediately)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			this.AddDependencies(input1);
			if (reactImmediately)
				InvokeReaction(Input1.GetValue());
		}

		public string Name { get; }
		public Action<TInput1> Reaction { get; }
		public Dispatcher Dispatcher { get; }
		/// <summary>
		/// Input number 1.
		/// </summary>
		public IPipelineStage<TInput1> Input1 { get; }

		public void OnInvalidate(IPipelineInvalidator invalidator)
		{
			invalidator.InvalidateAllDependentStages(this);
			InvokeReaction(Input1.GetValue());
		}

		private void InvokeReaction(TInput1 input1)
		{
			Dispatcher.Invoke(() => Reaction.Invoke(input1));
		}

		public override string ToString() => $"{Name} - Reaction is {Reaction.GetDetailedStringRepresentation()}";

		private static string GetDefaultName(params IPipelineStage[] stages) => $"Reaction to {string.Join(", ", stages.Select(p => "'" + p.Name + "'"))}";
    }

	public static partial class DispatcherReactions
	{
		public static IPipelineStage Create<TInput1>(
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1)
			=> new ReactionPipelineStage<TInput1>(
				reaction,
				input1);

		public static IPipelineStage Create<TInput1>(
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1>(
				reaction,
				input1,
				reactImmediately);

		public static IPipelineStage Create<TInput1>(
			string name, 
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1)
			=> new ReactionPipelineStage<TInput1>(
				name,
				reaction,
				input1);

		public static IPipelineStage Create<TInput1>(
			string name, 
			Action<TInput1> reaction, 
			IPipelineStage<TInput1> input1, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1>(
				name,
				reaction,
				input1,
				reactImmediately);
	}
}
