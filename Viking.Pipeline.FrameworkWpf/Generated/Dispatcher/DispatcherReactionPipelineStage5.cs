using System;
using System.Linq;
using System.Windows.Threading;

namespace Viking.Pipeline.Wpf
{
    public partial class DispatcherReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5> : IPipelineStage
    {
		public DispatcherReactionPipelineStage(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5) :
			this(GetDefaultName(input1, input2, input3, input4, input5), reaction, input1, input2, input3, input4, input5) 
		{ }

		public DispatcherReactionPipelineStage(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			bool reactImmediately) :
			this(GetDefaultName(input1, input2, input3, input4, input5), reaction, DispatcherUtilities.DefaultDispatcher, input1, input2, input3, input4, input5, reactImmediately) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5) :
			this(name, reaction, DispatcherUtilities.DefaultDispatcher, input1, input2, input3, input4, input5, true) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			Dispatcher dispatcher,
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5, 
			bool reactImmediately)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
			Input3 = input3 ?? throw new ArgumentNullException(nameof(input3));
			Input4 = input4 ?? throw new ArgumentNullException(nameof(input4));
			Input5 = input5 ?? throw new ArgumentNullException(nameof(input5));
			this.AddDependencies(input1, input2, input3, input4, input5);
			if (reactImmediately)
				InvokeReaction(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue());
		}

		public string Name { get; }
		public Action<TInput1, TInput2, TInput3, TInput4, TInput5> Reaction { get; }
		public Dispatcher Dispatcher { get; }
		/// <summary>
		/// Input number 1.
		/// </summary>
		public IPipelineStage<TInput1> Input1 { get; }
		/// <summary>
		/// Input number 2.
		/// </summary>
		public IPipelineStage<TInput2> Input2 { get; }
		/// <summary>
		/// Input number 3.
		/// </summary>
		public IPipelineStage<TInput3> Input3 { get; }
		/// <summary>
		/// Input number 4.
		/// </summary>
		public IPipelineStage<TInput4> Input4 { get; }
		/// <summary>
		/// Input number 5.
		/// </summary>
		public IPipelineStage<TInput5> Input5 { get; }

		public void OnInvalidate(IPipelineInvalidator invalidator)
		{
			invalidator.InvalidateAllDependentStages(this);
			InvokeReaction(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue());
		}

		private void InvokeReaction(TInput1 input1, TInput2 input2, TInput3 input3, TInput4 input4, TInput5 input5)
		{
			Dispatcher.Invoke(() => Reaction.Invoke(input1, input2, input3, input4, input5));
		}

		public override string ToString() => $"{Name} - Reaction is {Reaction.GetDetailedStringRepresentation()}";

		private static string GetDefaultName(params IPipelineStage[] stages) => $"Reaction to {string.Join(", ", stages.Select(p => "'" + p.Name + "'"))}";
    }

	public static partial class DispatcherReactions
	{
		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				reaction,
				input1, input2, input3, input4, input5);

		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				reaction,
				input1, input2, input3, input4, input5,
				reactImmediately);

		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				name,
				reaction,
				input1, input2, input3, input4, input5);

		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5>(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5>(
				name,
				reaction,
				input1, input2, input3, input4, input5,
				reactImmediately);
	}
}
