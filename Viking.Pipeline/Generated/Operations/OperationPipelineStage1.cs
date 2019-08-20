using System;

namespace Viking.Pipeline
{
    public class OperationPipelineStage<TInput1, TOutput> : IPipelineStage<TOutput>
    {
		public OperationPipelineStage(
			string name, 
			Func<TInput1, TOutput> operation, 
			IPipelineStage<TInput1> input1)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Operation = operation ?? throw new ArgumentNullException(nameof(operation));
			Input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
			this.AddDependencies(input1);
		}

		public string Name { get; }
		public Func<TInput1, TOutput> Operation { get; }
		public IPipelineStage<TInput1> Input1 { get; }

		public TOutput GetValue() => Operation(Input1.GetValue());

		public void OnInvalidate(IPipelineInvalidator invalidator) => invalidator.InvalidateAllDependentStages(this);

		public override string ToString() => $"{Name} - Operation is {Operation.GetDetailedStringRepresentation()}";
    }

	public static partial class PipelineOperations
	{
		public static IPipelineStage<TOutput> Create<TInput1, TOutput>(
			string name, 
			Func<TInput1, TOutput> operation, 
			IPipelineStage<TInput1> input1)
			=> new OperationPipelineStage<TInput1, TOutput>(
				name,
				operation,
				input1);
	}
}
