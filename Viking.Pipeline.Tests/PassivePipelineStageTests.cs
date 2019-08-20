using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class PassivePipelineStageTests
    {

        public void PassiveStageIsNeverInvalidated()
        {
            var assignable = new AssignablePipelineStage<int>("", 10);
            var passive = new PassivePipelineStage<int>(assignable);
            var test = passive.AttachTestStage();

            assignable.SetValue(11);
            test.AssertInvalidations(0);
        }
    }
}
