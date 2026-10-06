using NUnit.Framework;
using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    public sealed class BucketRulesFreeStepsTests
    {
        private BucketRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new BucketRules(BucketTestSettings.Create());
        }

        [Test]
        public void CapacitySteps_TenLoadAtFourStepsEach_IsFortySteps()
        {
            Assert.That(_rules.CapacitySteps, Is.EqualTo(40));
        }

        [Test]
        public void FreeStepsFor_PartlyLoaded_IsCapacityLeft()
        {
            Assert.That(_rules.FreeStepsFor(0), Is.EqualTo(40));
            Assert.That(_rules.FreeStepsFor(15), Is.EqualTo(25));
        }

        [Test]
        public void FreeStepsFor_Overfilled_IsZero()
        {
            Assert.That(_rules.FreeStepsFor(50), Is.EqualTo(0));
        }
    }
}
