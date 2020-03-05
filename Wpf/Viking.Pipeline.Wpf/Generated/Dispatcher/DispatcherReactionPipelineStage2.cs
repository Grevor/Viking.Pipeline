using System;
using System.Linq;
using System.Windows.Threading;

namespace Viking.Pipeline.Wpf
{
    public partial class DispatcherReactionPipelineStage<TInput1, TInput2> : IPipelineStage
    {
		public DispatcherReactionPipelineStage(
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2) :
			this(GetDefaultName(input1, input2), reaction, input1, input2) 
		{ }

		public DispatcherReactionPipelineStage(
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			bool reactImmediately) :
			this(GetDefaultName(input1, input2), reaction, DispatcherUtilities.DefaultDispatcher, input1, input2, reactImmediately) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2) :
			this(name, reaction, DispatcherUtilities.DefaultDispatcher, input1, input2, true) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2> reaction, 
			Dispatcher dispatcher,
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2, 
			bool reactImmediately)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
			this.AddDependencies(input1, input2);
			if (reactImmediately)
				InvokeReaction(Input1.GetValue(), Input2.GetValue());
		}

		public string Name { get; }
		public Action<TInput1, TInput2> Reaction { get; }
		public Dispatcher Dispatcher { get; }
		/// <summary>
		/// Input number 1.
		/// </summary>
		public IPipelineStage<TInput1> Input1 { get; }
		/// <summary>
		/// Input number 2.
		/// </summary>
		public IPipelineStage<TInput2> Input2 { get; }

		public void OnInvalidate(IPipelineInvalidator invalidator)
		{
			invalidator.InvalidateAllDependentStages(this);
			InvokeReaction(Input1.GetValue(), Input2.GetValue());
		}

		private void InvokeReaction(TInput1 input1, TInput2 input2)
		{
			Dispatcher.Invoke(() => Reaction.Invoke(input1, input2));
		}

		public override string ToString() => $"{Name} - Reaction is {Reaction.GetDetailedStringRepresentation()}";

		private static string GetDefaultName(params IPipelineStage[] stages) => $"Reaction to {string.Join(", ", stages.Select(p => "'" + p.Name + "'"))}";
    }

	public static partial class DispatcherReactions
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
