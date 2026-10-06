using NUnit.Framework;
using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    public sealed class BucketRulesUnloadTests
    {
        private BucketRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new BucketRules(BucketTestSettings.Create());
        }

        [Test]
        public void UnloadSteps_WithinLoad_UnloadsAllRequested()
        {
            Assert.That(_rules.UnloadSteps(20, 7), Is.EqualTo(7));
        }

        [Test]
        public void UnloadSteps_MoreThanLoad_UnloadsOnlyLoad()
        {
            Assert.That(_rules.UnloadSteps(5, 9), Is.EqualTo(5));
        }

        [Test]
        public void UnloadSteps_NegativeRequest_UnloadsNothing()
        {
            Assert.That(_rules.UnloadSteps(5, -3), Is.EqualTo(0));
        }
    }
}
