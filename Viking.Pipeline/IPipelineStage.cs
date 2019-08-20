namespace Viking.Pipeline
{
    public interface IPipelineStage
    {
        string Name { get; }
        void OnInvalidate(IPipelineInvalidator invalidator);
    }

    public interface IPipelineStage<TOutput> : IPipelineStage
    {
        TOutput GetValue();
    }
}
