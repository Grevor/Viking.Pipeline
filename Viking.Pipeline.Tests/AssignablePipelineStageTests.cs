using NUnit.Framework;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class AssignablePipelineStageTests
    {
        [TestCase(0)]
        [TestCase(4)]
        [TestCase(42)]
        public void InitialValueIsReflectedInGetValue(int initial)
        {
            var sut = new AssignablePipelineStage<int>("", initial);
            Assert.AreEqual(initial, sut.GetValue());
        }


        [Test]
        public void ExceptionOnNullInputToConstructor() => PipelineAssert.NullArgumentException(() => new AssignablePipelineStage<int>(null, 1), "name");

        [TestCase("name")]
        [TestCase("Another name")]
        public void GivenNameIsReflected(string name)
        {
            var sut = new AssignablePipelineStage<int>(name, 10);
            Assert.AreEqual(name, sut.Name);
        }

        [Test]
        public void SettingANewValueWillInvalidateTheStageAndAllDependencies()
        {
            var sut = Create(10);
            var test = sut.AttachTestStage();

            sut.SetValue(11);
            test.AssertStageInvalidated(sut);
            test.AssertInvalidations(1);
        }

        [TestCase(0, 1, true)]
        [TestCase(-2, 30, true)]
        [TestCase(2, 2, false)]
        [TestCase(30, 30, false)]
        public void SettingValueThroughAnyMeansWillResultInABooleanDenotingIfTheValueIsEqualOrNot(int initial, int value, bool expected)
        {
            var sut = Create(initial);
            Assert.AreEqual(expected, sut.SetValue(value));

            sut.SetValue(initial);
            Assert.AreEqual(expected, sut.SetValueWithoutInvalidating(value));
        }

        [TestCase(1)]
        [TestCase(5)]
        public void NewValueIsPropagatedToDependentStages(int newValue)
        {
            var sut = Create(0);
            var test = sut.AttachTestStage();

            sut.SetValue(newValue);
            PipelineAssert.Value(sut, newValue);
            PipelineAssert.Value(test, newValue);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-200)]
        public void SettingValueWithoutInvalidationDoesNotInvalidateTheStage(int a)
        {
            var sut = Create(0);
            var test = sut.AttachTestStage();
            sut.SetValueWithoutInvalidating(a);
            test.AssertInvalidations(0);
        }

        [Test]
        public void SettingTheValueToTheSameValueCausesNoInvalidations()
        {
            var sut = Create(22);
            var test = sut.AttachTestStage();

            sut.SetValue(22);
            test.AssertInvalidations(0);
        }

        [TestCase(-1)]
        [TestCase(200)]
        public void CustomComparerIsUsedWhenEvaluatingInvalidation(int value)
        {
            var comparer = new SettableEqualityComparer<int>(false);
            var sut = new AssignablePipelineStage<int>("", value, comparer);
            var test = sut.AttachTestStage();

            sut.SetValue(value);
            test.AssertInvalidations(1);
            test.AssertStageInvalidated(sut);

            comparer.Equal = true;
            test.PrepareForNext();
            sut.SetValue(value + 1);
            test.AssertInvalidations(1);
            test.AssertStageNotInvalidated(sut);
        }

        public static AssignablePipelineStage<T> Create<T>(T initial) => new AssignablePipelineStage<T>("sut", initial);
    }
}
