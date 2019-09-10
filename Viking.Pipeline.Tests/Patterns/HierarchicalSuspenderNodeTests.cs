using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Viking.Pipeline.Patterns;

namespace Viking.Pipeline.Tests.Patterns
{
    [TestFixture]
    public class HierarchicalSuspenderNodeTests
    {
        [Test]
        public void InitialNodeHasTheInputAsOutput()
        {
            var stage = PipelineTestHelper.Assignable(PipelineSuspensionState.Resume);
            var sut = new HierarchicalSuspenderNode(stage);

            Assert.AreSame(stage, sut.Output);
        }

        [TestCase(
            PipelineSuspensionState.Resume, 
            new[] { PipelineSuspensionState.ResumeWithoutPendingInvalidates, PipelineSuspensionState.Resume }, 
            new[] { HierarchicalBehavior.PropagateSuspendOnly, HierarchicalBehavior.PropagateSuspendOnly }, 
            PipelineSuspensionState.Resume,
            TestName = "PropagateSuspendOnly: Resume => ResumeWithoutPendingInvalidates => Resume = Resume")]
        [TestCase(
            PipelineSuspensionState.ResumeWithoutPendingInvalidates,
            new[] { PipelineSuspensionState.Resume, PipelineSuspensionState.Resume },
            new[] { HierarchicalBehavior.PropagateSuspendOnly, HierarchicalBehavior.PropagateSuspendOnly },
            PipelineSuspensionState.Resume,
            TestName = "PropagateSuspendOnly: ResumeWithoutPendingInvalidates => Resume => Resume = Resume")]

        [TestCase(
            PipelineSuspensionState.Resume,
            new[] { PipelineSuspensionState.ResumeWithoutPendingInvalidates, PipelineSuspensionState.ResumeWithoutPendingInvalidates },
            new[] { HierarchicalBehavior.PropagateSuspendOnly, HierarchicalBehavior.PropagateSuspendOnly },
            PipelineSuspensionState.ResumeWithoutPendingInvalidates,
            TestName = "PropagateSuspendOnly: Resume => ResumeWithoutPendingInvalidates => ResumeWithoutPendingInvalidates = ResumeWithoutPendingInvalidates")]
        [TestCase(
            PipelineSuspensionState.ResumeWithoutPendingInvalidates,
            new[] { PipelineSuspensionState.Resume, PipelineSuspensionState.ResumeWithoutPendingInvalidates },
            new[] { HierarchicalBehavior.PropagateSuspendOnly, HierarchicalBehavior.PropagateSuspendOnly },
            PipelineSuspensionState.ResumeWithoutPendingInvalidates,
            TestName = "PropagateSuspendOnly: ResumeWithoutPendingInvalidates => Resume => ResumeWithoutPendingInvalidates = ResumeWithoutPendingInvalidates")]


        [TestCase(
            PipelineSuspensionState.ResumeWithoutPendingInvalidates,
            new[] { PipelineSuspensionState.Resume, PipelineSuspensionState.Resume },
            new[] { HierarchicalBehavior.WeakenSuspensionState, HierarchicalBehavior.WeakenSuspensionState },
            PipelineSuspensionState.ResumeWithoutPendingInvalidates,
            TestName = "WeakenSuspensionState: ResumeWithoutPendingInvalidates => Resume => Resume = ResumeWithoutPendingInvalidates")]
        [TestCase(
            PipelineSuspensionState.Resume,
            new[] { PipelineSuspensionState.ResumeWithoutPendingInvalidates, PipelineSuspensionState.Resume },
            new[] { HierarchicalBehavior.WeakenSuspensionState, HierarchicalBehavior.WeakenSuspensionState },
            PipelineSuspensionState.ResumeWithoutPendingInvalidates,
            TestName = "WeakenSuspensionState: Resume => ResumeWithoutPendingInvalidates => Resume = ResumeWithoutPendingInvalidates")]

        [TestCase(
            PipelineSuspensionState.Suspend,
            new[] { PipelineSuspensionState.Resume, PipelineSuspensionState.Resume },
            new[] { HierarchicalBehavior.WeakenSuspensionState, HierarchicalBehavior.WeakenSuspensionState },
            PipelineSuspensionState.Suspend,
            TestName = "WeakenSuspensionState: Suspend => Resume => Resume = Suspend")]
        [TestCase(
            PipelineSuspensionState.Resume,
            new[] { PipelineSuspensionState.ResumeWithoutPendingInvalidates, PipelineSuspensionState.Suspend },
            new[] { HierarchicalBehavior.WeakenSuspensionState, HierarchicalBehavior.WeakenSuspensionState },
            PipelineSuspensionState.Suspend,
            TestName = "WeakenSuspensionState: Resume => ResumeWithoutPendingInvalidates => Suspend = Suspend")]

        public void HierarchyPropagateSuspensionStateAsExpected(
            PipelineSuspensionState root, 
            PipelineSuspensionState[] hierarchy, 
            HierarchicalBehavior[] behaviors, 
            PipelineSuspensionState expectedFinalState)
        {
            var parent = new HierarchicalSuspenderNode(root.AsPipelineConstant());
            var lastStage = hierarchy.Zip(behaviors, (a, b) => (input: a, behavior: b)).Aggregate(parent, (acc, next) => acc.CreateChild(next.input.AsPipelineConstant(), next.behavior));

            PipelineAssert.Value(lastStage.Output, expectedFinalState);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        public void InvalidationPropagatesThroughEntireHierarchy(int size)
        {
            var inputs = Enumerable.Repeat(PipelineSuspensionState.Resume, size + 1).Select(r => PipelineTestHelper.Assignable(r)).ToList();

            var stages = new List<HierarchicalSuspenderNode> { new HierarchicalSuspenderNode(inputs[0]) };

            for (int i = 0; i < size; ++i)
                stages.Add(stages[i].CreateChild(inputs[i + 1], HierarchicalBehavior.WeakenSuspensionState));

            var tests = stages.Select(s => s.Output.AttachTestStage()).ToList();

            for(int i = 0; i < size; ++i)
            {
                inputs[i].SetValue(PipelineSuspensionState.Suspend);
                foreach(var test in tests.Skip(i))
                    test.AssertInvalidations(i + 1);
                PipelineAssert.Value(stages.Last().Output, PipelineSuspensionState.Suspend);
            }
        }

        [TestCase(PipelineSuspensionState.Resume, true)]
        [TestCase(PipelineSuspensionState.ResumeWithoutPendingInvalidates, true)]
        [TestCase(PipelineSuspensionState.Suspend, false)]
        public void CreatingSuspenderStageFromHierarchyWillGetHierarchySuspensionState(PipelineSuspensionState state, bool expectInvalidation)
        {
            var hierarchy = new HierarchicalSuspenderNode(state.AsPipelineConstant());

            var input = PipelineTestHelper.Assignable(1);
            var sut = hierarchy.WithSuspender(input);
            var test = sut.AttachTestStage();

            input.SetValue(10);

            if (expectInvalidation)
                test.AssertInvalidations(1);
            else
                test.AssertInvalidations(0);
        }
    }
}
