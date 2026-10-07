using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class RamWatchTests
    {
        private RamWatch _watch;

        [SetUp]
        public void SetUp()
        {
            _watch = new RamWatch();
        }

        [Test]
        public void Observe_FirstCounts_TakesBaselineWithoutRole()
        {
            Assert.That(_watch.Observe(3, 2), Is.EqualTo(RamRole.None));
        }

        [Test]
        public void Observe_UnchangedCounts_ReturnsNone()
        {
            _watch.Observe(1, 1);

            Assert.That(_watch.Observe(1, 1), Is.EqualTo(RamRole.None));
        }

        [Test]
        public void Observe_TimesRammedRises_ReturnsVictim()
        {
            _watch.Observe(0, 0);

            Assert.That(_watch.Observe(1, 0), Is.EqualTo(RamRole.Victim));
        }

        [Test]
        public void Observe_RamsDealtRises_ReturnsRammer()
        {
            _watch.Observe(0, 0);

            Assert.That(_watch.Observe(0, 1), Is.EqualTo(RamRole.Rammer));
        }

        [Test]
        public void Observe_BothRiseTogether_ReturnsVictim()
        {
            _watch.Observe(0, 0);

            Assert.That(_watch.Observe(1, 1), Is.EqualTo(RamRole.Victim));
        }

        [Test]
        public void Observe_SameRiseSeenTwice_FiresOnce()
        {
            _watch.Observe(0, 0);
            _watch.Observe(1, 0);

            Assert.That(_watch.Observe(1, 0), Is.EqualTo(RamRole.None));
        }

        [Test]
        public void Observe_CountDropsThenReturns_DoesNotFireAgain()
        {
            _watch.Observe(0, 0);
            _watch.Observe(1, 0);
            _watch.Observe(0, 0);

            Assert.That(_watch.Observe(1, 0), Is.EqualTo(RamRole.None));
        }

        [Test]
        public void Observe_VictimAfterRammerRise_ReturnsVictim()
        {
            _watch.Observe(0, 0);
            _watch.Observe(0, 1);

            Assert.That(_watch.Observe(1, 1), Is.EqualTo(RamRole.Victim));
        }

        [Test]
        public void Reset_ThenObserve_TakesNewBaseline()
        {
            _watch.Observe(5, 5);
            _watch.Reset();

            Assert.That(_watch.Observe(0, 0), Is.EqualTo(RamRole.None));
            Assert.That(_watch.Observe(1, 0), Is.EqualTo(RamRole.Victim));
        }
    }
}
