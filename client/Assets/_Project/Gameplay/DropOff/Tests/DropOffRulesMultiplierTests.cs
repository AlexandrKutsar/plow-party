using NUnit.Framework;
using PlowParty.Gameplay.DropOff.Simulation;

namespace PlowParty.Gameplay.DropOff.Tests
{
    public sealed class DropOffRulesMultiplierTests
    {
        private DropOffRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = DropOffTestSettings.CreateRules();
        }

        [Test]
        public void MultiplierFor_EmptyBucket_IsZero()
        {
            Assert.That(_rules.MultiplierFor(0), Is.EqualTo(0f));
        }

        [Test]
        public void MultiplierFor_LessThanOneLoad_IsBase()
        {
            Assert.That(_rules.MultiplierFor(1), Is.EqualTo(1f));
        }

        [TestCase(1)]
        [TestCase(50)]
        public void MultiplierFor_UpToFifty_IsBase(int load)
        {
            Assert.That(_rules.MultiplierFor(StepsOf(load)), Is.EqualTo(1f));
        }

        [TestCase(51)]
        [TestCase(99)]
        public void MultiplierFor_FiftyOneToNinetyNine_IsOneAndHalf(int load)
        {
            Assert.That(_rules.MultiplierFor(StepsOf(load)), Is.EqualTo(1.5f));
        }

        [Test]
        public void MultiplierFor_AlmostFullBucket_IsOneAndHalf()
        {
            Assert.That(_rules.MultiplierFor(DropOffTestSettings.FullBucketSteps - 1), Is.EqualTo(1.5f));
        }

        [Test]
        public void MultiplierFor_FullBucket_IsDouble()
        {
            Assert.That(_rules.MultiplierFor(DropOffTestSettings.FullBucketSteps), Is.EqualTo(2f));
        }

        [Test]
        public void MultiplierFor_TiersOutOfOrder_PicksHighestReachedTier()
        {
            var settings = DropOffTestSettings.Create();
            settings.MultiplierTiers = new[] { new MultiplierTier(100, 2f), new MultiplierTier(51, 1.5f) };
            var rules = new DropOffRules(settings, DropOffTestSettings.CreateBucket());

            Assert.That(rules.MultiplierFor(DropOffTestSettings.FullBucketSteps), Is.EqualTo(2f));
            Assert.That(rules.MultiplierFor(StepsOf(60)), Is.EqualTo(1.5f));
        }

        private static int StepsOf(int load)
        {
            return load * DropOffTestSettings.StepsPerLoad;
        }
    }
}
