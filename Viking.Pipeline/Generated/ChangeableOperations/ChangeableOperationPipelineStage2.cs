using System;

namespace Viking.Pipeline
{
    public class ChangeableOperationPipelineStage<TInput1, TInput2, TOutput> : IPipelineStage<TOutput>
    {
		public ChangeableOperationPipelineStage(
			string name, 
			IPipelineStage<Func<TInput1, TInput2, TOutput>> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			OperationStage = operation ?? throw new ArgumentNullException(nameof(operation));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			Input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
			this.AddDependencies(operation, input1, input2);
		}

		public string Name { get; }
		public IPipelineStage<Func<TInput1, TInput2, TOutput>> OperationStage { get; }
		public IPipelineStage<TInput1> Input1 { get; }
		public IPipelineStage<TInput2> Input2 { get; }

		public TOutput GetValue() => OperationStage.GetValue().Invoke(Input1.GetValue(), Input2.GetValue());

		public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

		public override string ToString() => $"{Name} - Operation is {OperationStage.GetValue()?.GetDetailedStringRepresentation() ?? "<null>" }";
    }

	public static partial class ChangeablePipelineOperations
	{
		public static ChangeableOperationPipelineStage<TInput1, TInput2, TOutput> Create<TInput1, TInput2, TOutput>(
			string name, 
			IPipelineStage<Func<TInput1, TInput2, TOutput>> operation, 
			IPipelineStage<TInput1> input1,
			IPipelineStage<TInput2> input2)
			=> new ChangeableOperationPipelineStage<TInput1, TInput2, TOutput>(
				name,
				operation,
				input1, input2);
	}
}
