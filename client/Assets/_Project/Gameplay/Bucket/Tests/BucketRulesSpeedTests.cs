using NUnit.Framework;
using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    public sealed class BucketRulesSpeedTests
    {
        private BucketRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new BucketRules(BucketTestSettings.Create());
        }

        [Test]
        public void SpeedMultiplier_Empty_IsFullSpeed()
        {
            Assert.That(_rules.SpeedMultiplier(0), Is.EqualTo(1f));
        }

        [Test]
        public void SpeedMultiplier_Full_LosesMaxPenalty()
        {
            Assert.That(_rules.SpeedMultiplier(40), Is.EqualTo(0.8f).Within(1e-5f));
        }

        [Test]
        public void SpeedMultiplier_HalfFull_LosesHalfThePenalty()
        {
            Assert.That(_rules.SpeedMultiplier(20), Is.EqualTo(0.9f).Within(1e-5f));
        }
    }
}
