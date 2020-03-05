using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class DataRetrievalPipelineStageTests
    {

        [Test]
        public void ExceptionOnNullToConstructor()
        {
            PipelineAssert.NullArgumentException(() => new DataRetrievalPipelineStage<int>(null, () => 1), "name");
            PipelineAssert.NullArgumentException(() => new DataRetrievalPipelineStage<int>("name", null), "source");
            PipelineAssert.NullArgumentException(() => new DataRetrievalPipelineStage<int>(null, null), "name and source");
        }

        [Test]
        public void DataRetrievalAlwaysCallsTheSpecifiedFunctionOnGetValueCall()
        {
            var value = new ValueRepository(2);

            var sut = new DataRetrievalPipelineStage<int>("name", value.GetValue);

            PipelineAssert.Value(sut, value.Value);
            Assert.AreEqual(1, value.NumCalls);

            sut.GetValue();
            sut.GetValue();
            sut.GetValue();
            sut.GetValue();
            Assert.AreEqual(5, value.NumCalls);
        }
    }

    internal class ValueRepository
    {
        public ValueRepository(int value) => Value = value;

        public int Value { get; set; }
        public int NumCalls { get; private set; }

        public int GetValue()
        {
            ++NumCalls;
            return Value;
        }
    }
}
