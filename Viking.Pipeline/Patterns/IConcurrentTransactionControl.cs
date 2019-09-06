using System.Collections.Generic;

namespace Viking.Pipeline.Patterns
{
    public interface IConcurrentTransactionControl
    {
        long GetTimestamp();
        PipelineTransactionCommitResult Commit(IEnumerable<ConcurrentTransactionPart> parts);
    }
}
