using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viking.Pipeline.Tests
{
    [TestFixture]
    public class MultiplexingPipelineStageTests
    {
        [Test]
        public void ExceptionOnNullInputToConstructor()
        {
            var name = "Name";
            var select = 1.AsPipelineConstant();
            var inputs = new Dictionary<int, IPipelineStage<int>> { };
            var comparer = EqualityComparer<int>.Default;

            PipelineAssert.NullArgumentException(() => new MultiplexingPipelineStage<int, int>(null, select, inputs), "name");
            PipelineAssert.NullArgumentException(() => new MultiplexingPipelineStage<int, int>(null, select, inputs, comparer), "name");

            PipelineAssert.NullArgumentException(() => new MultiplexingPipelineStage<int, int>(name, null, inputs), "select");
            PipelineAssert.NullArgumentException(() => new MultiplexingPipelineStage<int, int>(name, null, inputs, comparer), "select");

            PipelineAssert.NullArgumentException(() => new MultiplexingPipelineStage<int, int>(name, select, null), "inputs");
            PipelineAssert.NullArgumentException(() => new MultiplexingPipelineStage<int, int>(name, select, null, comparer), "inputs");

            PipelineAssert.NullArgumentException(() => new MultiplexingPipelineStage<int, int>(name, select, inputs, null), "comparer");
        }

        [Test]
        public void DependenciesToInputsAndSelectAreRegisteredCorrectly()
        {
            var select = 0.AsPipelineConstant();

            var input1 = 2.AsPipelineConstant();
            var input2 = 4.AsPipelineConstant();
            var inputs = CreateInputs(input1, input2);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            PipelineAssert.DependentOn(sut, select, input1, input2);
        }

        [Test]
        public void ExceptionIsThrownOnInvalidateWhenTheSelectedStageDoesNotExistInInputs()
        {
            var select = 0.AsPipelineConstant();
            var inputs = new Dictionary<int, IPipelineStage<int>> { { 1, 1.AsPipelineConstant() } };

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            Assert.Catch(() => sut.Invalidate());
        }

        [Test]
        public void ExceptionIsThrownOnGetValueWhenTheSelectedStageDoesNotExistInInputs()
        {
            var select = 0.AsPipelineConstant();
            var inputs = new Dictionary<int, IPipelineStage<int>> { { 1, 1.AsPipelineConstant() } };

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            Assert.Catch(() => sut.GetValue());
        }

        [Test]
        public void StageIsInvalidatedWhenSelectSignalChanges()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var inputs = CreateInputs(2.AsPipelineConstant(), 4.AsPipelineConstant());

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);
            var test = sut.AttachTestStage();

            select.SetValue(1);
            select.SetValue(0);

            test.AssertInvalidations(2);
        }

        [Test]
        public void StageInvalidatesDependentStages()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var inputs = CreateInputs(2.AsPipelineConstant(), 4.AsPipelineConstant());

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);
            PipelineAssert.AssertPipelineIsInvalidatingDependentStages(sut);
        }

        [Test]
        public void StageIsInvalidatedWhenCurrentlySelectedInputChanges()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input = AssignablePipelineStage.Create("Input", 0);
            var inputs = CreateInputs(input);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);
            var test = sut.AttachTestStage();

            input.SetValue(1);
            test.AssertInvalidations(1);
        }

        [Test]
        public void StageIsNotInvalidatedWhenAnUnselectedInputChanges()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var input1 = AssignablePipelineStage.Create("Input 1", 0);
            var inputs = CreateInputs(input0, input1);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);
            var test = sut.AttachTestStage();

            input1.SetValue(1);
            test.AssertInvalidations(0);
        }

        [TestCase(0, new[] { 2, 3, 4 }, 2)]
        [TestCase(1, new[] { 2, 3, 4 }, 3)]
        [TestCase(2, new[] { 2, 3, 4 }, 4)]
        [TestCase(0, new[] { 7 }, 7)]
        public void OutputIsTakenFromTheSelectedInput(int selected, int[] inputValues, int expected)
        {
            var select = AssignablePipelineStage.Create("Select", selected);
            var inputs = CreateInputs(inputValues.Select(i => i.AsPipelineConstant()).ToArray());

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);
            PipelineAssert.Value(sut, expected);
        }

        [Test]
        public void SettingANewInputForTheCurrentlySelectedInputCausesInvalidationAndGetsTheNewValue()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var input1 = AssignablePipelineStage.Create("Input 1", 0);
            var newInput0 = AssignablePipelineStage.Create("Input 0 (new)", 100);
            var inputs = CreateInputs(input0, input1);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);
            var test = sut.AttachTestStage();

            sut.SetInput(0, newInput0);

            test.AssertInvalidations(1);
            PipelineAssert.Value(sut, 100);
        }

        [Test]
        public void SettingANewInputForAnUnselectedInputCausesNoInvalidation()
        {
            var select = AssignablePipelineStage.Create("Select", 1);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var input1 = AssignablePipelineStage.Create("Input 1", 0);
            var newInput0 = AssignablePipelineStage.Create("Input 0 (new)", 100);
            var inputs = CreateInputs(input0, input1);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);
            var test = sut.AttachTestStage();

            sut.SetInput(0, newInput0);

            test.AssertInvalidations(0);
        }

        [Test]
        public void RemovingTheCurrentlySelectedInputCausesException()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var inputs = CreateInputs(input0);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            Assert.Catch(() => sut.RemoveInput(0));
        }

        [Test]
        public void RemovingAStageRemovesDependency()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var input1 = AssignablePipelineStage.Create("Input 1", 0);
            var inputs = CreateInputs(input0, input1);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            sut.RemoveInput(1);
            PipelineAssert.NotDependentOn(sut, input1);
        }

        [Test]
        public void SettingANewMappingRemovesOldDependencyAndAddsNew()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var newInput0 = AssignablePipelineStage.Create("Input 0 (new)", 0);
            var inputs = CreateInputs(input0);
            
            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            sut.SetInput(0, newInput0);

            PipelineAssert.NotDependentOn(sut, input0);
            PipelineAssert.DependentOn(sut, newInput0);
        }

        [Test]
        public void DependencyIsOnlyRemovedIfAllInputsDependentOnThatStageAreRemoved()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var input2 = AssignablePipelineStage.Create("Input 2", 0);
            var inputs = CreateInputs(input0, input0, input2);
            
            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            sut.RemoveInput(1);
            PipelineAssert.DependentOn(sut, input0, input2);
            select.SetValue(2); // To prevent exception
            sut.RemoveInput(0);
            PipelineAssert.NotDependentOn(sut, input0);
            PipelineAssert.DependentOn(sut, input2);
        }

        [Test]
        public void DependencyIsOnlyRemovedIfAllInputsDependentOnThatStageAreChanged()
        {
            var select = AssignablePipelineStage.Create("Select", 0);
            var input0 = AssignablePipelineStage.Create("Input 0", 0);
            var newInput0 = AssignablePipelineStage.Create("Input 0 (new)", 0);
            var inputs = CreateInputs(input0, input0);

            var sut = new MultiplexingPipelineStage<int, int>("Name", select, inputs);

            sut.SetInput(0, newInput0);
            PipelineAssert.DependentOn(sut, input0);
            sut.SetInput(1, newInput0);
            PipelineAssert.NotDependentOn(sut, input0);
        }

        private Dictionary<int, IPipelineStage<int>> CreateInputs(params IPipelineStage<int>[] inputs)
            => Enumerable.Range(0, inputs.Length).ToDictionary(i => i, i => inputs[i]);
    }
}
