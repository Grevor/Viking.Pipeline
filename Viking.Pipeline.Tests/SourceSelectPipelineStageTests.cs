using NUnit.Framework;
using System;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class SourceSelectPipelineStageTests
    {

        [Test]
        public void ExceptionOnNullInputToConstructor()
        {
            PipelineAssert.NullArgumentException(() => new SourceSelectPipelineStage<int>(null, 1.AsPipelineConstant()), "name");
            PipelineAssert.NullArgumentException(() => new SourceSelectPipelineStage<int>("", null), "initial");
        }

        [Test]
        public void InitialSourceIsAddedAsDependency()
        {
            var input = 1.AsPipelineConstant();
            var sut = new SourceSelectPipelineStage<int>("", input);

            PipelineAssert.DependentOn(sut, input);
        }

        [Test]
        public void SettingANewSourceRemovesOldDependencyANdAddsNew()
        {
            var input = 1.AsPipelineConstant();
            var sut = new SourceSelectPipelineStage<int>("", input);

            PipelineAssert.DependentOn(sut, input);

            var input2 = 2.AsPipelineConstant();
            sut.SetSource(input2);
            PipelineAssert.NotDependentOn(sut, input);
            PipelineAssert.DependentOn(sut, input2);

            var input3 = 3.AsPipelineConstant();
            sut.SetSourceWithoutInvalidating(input3);
            PipelineAssert.NotDependentOn(sut, input2);
            PipelineAssert.DependentOn(sut, input3);
        }

        [Test]
        public void SourceSelectDoesNotAcceptNullAsInputToConstructor()
        {
            Assert.Throws<ArgumentNullException>(() => new SourceSelectPipelineStage<string>("", null));
        }

        [Test]
        public void SettingANewSourceCausesInvalidation()
        {
            var sut = new SourceSelectPipelineStage<string>("", "".AsPipelineConstant());
            var test = sut.AttachTestStage();

            sut.SetSource("Derpface".AsPipelineConstant());
            test.AssertInvalidations(1);
        }

        [Test]
        public void SettingANewSourceWithoutInvalidateCausesNoInvalidation()
        {
            var sut = new SourceSelectPipelineStage<string>("", "".AsPipelineConstant());
            var test = sut.AttachTestStage();

            sut.SetSourceWithoutInvalidating("Derpface".AsPipelineConstant());
            test.AssertInvalidations(0);
        }

        [Test]
        public void ValueIsTakenFromCurrentSource()
        {
            var sut = new SourceSelectPipelineStage<string>("", "Hello, World?".AsPipelineConstant());
            PipelineAssert.Value(sut, "Hello, World?");

            sut.SetSource("Whazzup?".AsPipelineConstant());
            PipelineAssert.Value(sut, "Whazzup?");

            sut.SetSourceWithoutInvalidating("Yo!".AsPipelineConstant());
            PipelineAssert.Value(sut, "Yo!");
        }

        [Test]
        public void SettingTheCurrentSourceAsNewSourceCausesNoInvalidation()
        {
            var source = 3.AsPipelineConstant();
            var sut = new SourceSelectPipelineStage<int>("", source);
            var test = sut.AttachTestStage();

            sut.SetSource(source);
            test.AssertInvalidations(0);
        }

    }
}
