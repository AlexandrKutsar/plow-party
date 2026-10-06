using NUnit.Framework;
using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    public sealed class BucketRulesCollectTests
    {
        private BucketRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new BucketRules(BucketTestSettings.Create());
        }

        [Test]
        public void Collect_WithinCapacity_AddsAllSteps()
        {
            Assert.That(_rules.Collect(10, 7), Is.EqualTo(17));
        }

        [Test]
        public void Collect_PastCapacity_StopsAtCapacity()
        {
            Assert.That(_rules.Collect(35, 12), Is.EqualTo(40));
        }
    }
}
