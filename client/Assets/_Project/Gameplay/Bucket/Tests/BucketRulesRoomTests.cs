using NUnit.Framework;
using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    public sealed class BucketRulesRoomTests
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
        public void RoomFor_PartlyLoaded_IsCapacityLeft()
        {
            Assert.That(_rules.RoomFor(0), Is.EqualTo(40));
            Assert.That(_rules.RoomFor(15), Is.EqualTo(25));
        }

        [Test]
        public void RoomFor_Overfilled_IsZero()
        {
            Assert.That(_rules.RoomFor(50), Is.EqualTo(0));
        }
    }
}
