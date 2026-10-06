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

        [Test]
        public void IsFull_OnlyAtCapacity()
        {
            Assert.That(_rules.IsFull(39), Is.False);
            Assert.That(_rules.IsFull(40), Is.True);
        }

        [Test]
        public void LoadUnits_PartialLoad_RoundsDown()
        {
            Assert.That(_rules.LoadUnits(3), Is.EqualTo(0));
            Assert.That(_rules.LoadUnits(7), Is.EqualTo(1));
            Assert.That(_rules.LoadUnits(40), Is.EqualTo(10));
        }
    }
}
