using NUnit.Framework;
using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    public sealed class BucketRulesSpillTests
    {
        private BucketRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new BucketRules(BucketTestSettings.Create());
        }

        [Test]
        public void SpillSteps_EvenShare_TakesThatShare()
        {
            Assert.That(_rules.SpillSteps(30), Is.EqualTo(9));
        }

        [Test]
        public void SpillSteps_FractionalShare_RoundsUp()
        {
            Assert.That(_rules.SpillSteps(11), Is.EqualTo(4));
        }

        [Test]
        public void SpillSteps_SingleStep_SpillsIt()
        {
            Assert.That(_rules.SpillSteps(1), Is.EqualTo(1));
        }

        [Test]
        public void SpillSteps_WholeShare_NeverExceedsLoad()
        {
            var settings = BucketTestSettings.Create();
            settings.SpillShare = 1f;

            Assert.That(new BucketRules(settings).SpillSteps(13), Is.EqualTo(13));
        }

        [Test]
        public void SpillSteps_EmptyBucket_IsZero()
        {
            Assert.That(_rules.SpillSteps(0), Is.EqualTo(0));
        }
    }
}
