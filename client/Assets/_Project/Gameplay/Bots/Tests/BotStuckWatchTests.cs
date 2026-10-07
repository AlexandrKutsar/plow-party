using NUnit.Framework;
using PlowParty.Gameplay.Bots.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class BotStuckWatchTests
    {
        private BotStuckWatch _watch;

        [SetUp]
        public void SetUp()
        {
            _watch = new BotStuckWatch(BotTestSettings.Create());
        }

        [Test]
        public void Observe_MakingProgress_IsNotStuck()
        {
            for (var tick = 0; tick <= 30; tick++)
            {
                Assert.That(_watch.Observe(new Vector2(tick * 0.1f, 0f), tick * 0.1f, true), Is.False);
            }
        }

        [Test]
        public void Observe_NoProgressPastStuckTime_IsStuck()
        {
            _watch.Observe(Vector2.zero, 0f, true);

            Assert.That(_watch.Observe(new Vector2(0.2f, 0f), 0.9f, true), Is.False);
            Assert.That(_watch.Observe(new Vector2(0.3f, 0f), 1.1f, true), Is.True);
        }

        [Test]
        public void Observe_StandingStillOnPurpose_IsNeverStuck()
        {
            _watch.Observe(Vector2.zero, 0f, false);

            Assert.That(_watch.Observe(Vector2.zero, 5f, false), Is.False);
            Assert.That(_watch.StillFor(5f), Is.EqualTo(0f));
        }

        [Test]
        public void StillFor_WhileStuck_GrowsUntilTheVehicleMoves()
        {
            _watch.Observe(Vector2.zero, 0f, true);
            _watch.Observe(Vector2.zero, 2f, true);

            Assert.That(_watch.StillFor(3f), Is.EqualTo(3f));

            _watch.Observe(new Vector2(1f, 0f), 3.1f, true);

            Assert.That(_watch.StillFor(3.1f), Is.EqualTo(0f));
        }

        [Test]
        public void Stop_AfterALongStill_ForgetsIt()
        {
            _watch.Observe(Vector2.zero, 0f, true);
            _watch.Observe(Vector2.zero, 4f, true);

            _watch.Stop();

            Assert.That(_watch.StillFor(10f), Is.EqualTo(0f));
            Assert.That(_watch.Observe(Vector2.zero, 10f, true), Is.False);
        }

        [Test]
        public void Restart_AfterStuck_GivesAFreshWindow()
        {
            _watch.Observe(Vector2.zero, 0f, true);
            Assume.That(_watch.Observe(Vector2.zero, 1.5f, true), Is.True);

            _watch.Restart(Vector2.zero, 1.5f);

            Assert.That(_watch.Observe(Vector2.zero, 2f, true), Is.False);
        }
    }
}
