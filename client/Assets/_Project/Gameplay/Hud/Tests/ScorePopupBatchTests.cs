using NUnit.Framework;
using PlowParty.Gameplay.Hud.Simulation;

namespace PlowParty.Gameplay.Hud.Tests
{
    public sealed class ScorePopupBatchTests
    {
        private ScorePopupBatch _rise;

        [SetUp]
        public void SetUp()
        {
            _rise = new ScorePopupBatch(0.25f);
        }

        [Test]
        public void Observe_FirstScore_ShowsNothing()
        {
            Assert.That(_rise.Observe(120, 0.1f), Is.EqualTo(0));
        }

        [Test]
        public void Observe_UnchangedScore_ShowsNothing()
        {
            _rise.Observe(10, 0.1f);

            Assert.That(_rise.Observe(10, 1f), Is.EqualTo(0));
        }

        [Test]
        public void Observe_FirstRise_ShowsAtOnce()
        {
            _rise.Observe(10, 0.1f);

            Assert.That(_rise.Observe(13, 0.02f), Is.EqualTo(3));
        }

        [Test]
        public void Observe_RisesWithinInterval_AddUpIntoNextPopup()
        {
            _rise.Observe(0, 0.1f);
            _rise.Observe(2, 0.02f);

            Assert.That(_rise.Observe(3, 0.1f), Is.EqualTo(0));
            Assert.That(_rise.Observe(5, 0.1f), Is.EqualTo(0));
            Assert.That(_rise.Observe(6, 0.1f), Is.EqualTo(4));
        }

        [Test]
        public void Observe_ScoreDrops_RestartsFromNewScore()
        {
            _rise.Observe(50, 0.1f);

            Assert.That(_rise.Observe(0, 0.1f), Is.EqualTo(0));
            Assert.That(_rise.Observe(4, 0.1f), Is.EqualTo(4));
        }

        [Test]
        public void Forget_ThenObserve_TreatsScoreAsFirst()
        {
            _rise.Observe(10, 0.1f);
            _rise.Forget();

            Assert.That(_rise.Observe(40, 0.1f), Is.EqualTo(0));
        }
    }
}
