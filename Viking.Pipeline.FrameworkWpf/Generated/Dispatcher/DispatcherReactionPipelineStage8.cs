using System;
using System.Linq;
using System.Windows.Threading;

namespace Viking.Pipeline.Wpf
{
    public partial class DispatcherReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> : IPipelineStage
    {
		public DispatcherReactionPipelineStage(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8) :
			this(GetDefaultName(input1, input2, input3, input4, input5, input6, input7, input8), reaction, input1, input2, input3, input4, input5, input6, input7, input8) 
		{ }

		public DispatcherReactionPipelineStage(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8,
			bool reactImmediately) :
			this(GetDefaultName(input1, input2, input3, input4, input5, input6, input7, input8), reaction, DispatcherUtilities.DefaultDispatcher, input1, input2, input3, input4, input5, input6, input7, input8, reactImmediately) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8) :
			this(name, reaction, DispatcherUtilities.DefaultDispatcher, input1, input2, input3, input4, input5, input6, input7, input8, true) 
		{ }

		public DispatcherReactionPipelineStage(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			Dispatcher dispatcher,
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8, 
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
			Input6 = input6 ?? throw new ArgumentNullException(nameof(input6));
			Input7 = input7 ?? throw new ArgumentNullException(nameof(input7));
			Input8 = input8 ?? throw new ArgumentNullException(nameof(input8));
			this.AddDependencies(input1, input2, input3, input4, input5, input6, input7, input8);
			if (reactImmediately)
				InvokeReaction(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue(), Input6.GetValue(), Input7.GetValue(), Input8.GetValue());
		}

		public string Name { get; }
		public Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> Reaction { get; }
		public Dispatcher Dispatcher { get; }
		public IPipelineStage<TInput1> Input1 { get; }
		public IPipelineStage<TInput2> Input2 { get; }
		public IPipelineStage<TInput3> Input3 { get; }
		public IPipelineStage<TInput4> Input4 { get; }
		public IPipelineStage<TInput5> Input5 { get; }
		public IPipelineStage<TInput6> Input6 { get; }
		public IPipelineStage<TInput7> Input7 { get; }
		public IPipelineStage<TInput8> Input8 { get; }

		public void OnInvalidate(IPipelineInvalidator invalidator)
		{
			invalidator.InvalidateAllDependentStages(this);
			InvokeReaction(Input1.GetValue(), Input2.GetValue(), Input3.GetValue(), Input4.GetValue(), Input5.GetValue(), Input6.GetValue(), Input7.GetValue(), Input8.GetValue());
		}

		private void InvokeReaction(TInput1 input1, TInput2 input2, TInput3 input3, TInput4 input4, TInput5 input5, TInput6 input6, TInput7 input7, TInput8 input8)
		{
			Dispatcher.Invoke(() => Reaction.Invoke(input1, input2, input3, input4, input5, input6, input7, input8));
		}

		public override string ToString() => $"{Name} - Reaction is {Reaction.GetDetailedStringRepresentation()}";

		private static string GetDefaultName(params IPipelineStage[] stages) => $"Reaction to {string.Join(", ", stages.Select(p => "'" + p.Name + "'"))}";
    }

	public static partial class DispatcherReactions
	{
		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
				reaction,
				input1, input2, input3, input4, input5, input6, input7, input8);

		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
				reaction,
				input1, input2, input3, input4, input5, input6, input7, input8,
				reactImmediately);

		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
				name,
				reaction,
				input1, input2, input3, input4, input5, input6, input7, input8);

		public static IPipelineStage Create<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
			string name, 
			Action<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8> reaction, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2,
			IPipelineStage<TInput3> input3,
			IPipelineStage<TInput4> input4,
			IPipelineStage<TInput5> input5,
			IPipelineStage<TInput6> input6,
			IPipelineStage<TInput7> input7,
			IPipelineStage<TInput8> input8, 
			bool reactImmediately)
			=> new ReactionPipelineStage<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TInput8>(
				name,
				reaction,
				input1, input2, input3, input4, input5, input6, input7, input8,
				reactImmediately);
	}
}
