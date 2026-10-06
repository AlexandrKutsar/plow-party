using NUnit.Framework;
using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    public sealed class BucketRulesLoadTests
    {
        private BucketRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new BucketRules(BucketTestSettings.Create());
        }

        [Test]
        public void IsFull_BelowOrAtCapacity_TrueOnlyAtCapacity()
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
