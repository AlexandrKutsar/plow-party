using NUnit.Framework;
using PlowParty.Gameplay.Bots.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class BotBrainTests
    {
        private const int Self = 0;
        private const float Tick = 1f / 64f;

        private BotWorld _world;
        private BotBrain _brain;
        private float _clock;

        [SetUp]
        public void SetUp()
        {
            _world = new BotWorld(BotTestSettings.OpenGrid(), Vector2.zero, 3.5f, 100, new[] { 51, 100 }, 6);
            _world.PlayingRemaining = 120f;
            _brain = new BotBrain(BotTestSettings.Profile(), BotTestSettings.Create(), 1, 6);
            _brain.Reset(0f);
            _clock = 0f;
        }

        [Test]
        public void Step_EmptyBucketNextToSnow_DrivesIntoTheSnow()
        {
            _world.Snow.Refresh(new FakeSnowDepths(0).Fill(new Vector2(4f, 4f), new Vector2(8f, 8f), 3));
            Place(new Vector2(-2f, -2f), Vector2.up, 0);

            var stick = Run(1f);

            Assert.That(_brain.Action, Is.EqualTo(BotAction.Collect));
            Assert.That(Vector2.Dot(stick.normalized, new Vector2(1f, 1f).normalized), Is.GreaterThan(0.7f));
        }

        [Test]
        public void Step_FullBucket_HeadsForTheDropOffZone()
        {
            _world.Snow.Refresh(new FakeSnowDepths(3));
            Place(new Vector2(8f, 0f), Vector2.right, 100);

            var stick = Run(1f);

            Assert.That(_brain.Action, Is.EqualTo(BotAction.Deliver));
            Assert.That(stick.x, Is.LessThan(0f));
        }

        [Test]
        public void Step_InTheZoneWithLoad_StandsStillToUnload()
        {
            _world.Snow.Refresh(new FakeSnowDepths(3));
            Place(new Vector2(6f, 0f), Vector2.left, 100);
            Run(1f);
            Assume.That(_brain.Action, Is.EqualTo(BotAction.Deliver));

            Place(new Vector2(2.5f, 0f), Vector2.left, 100);
            var stick = Run(0.5f);

            Assert.That(stick, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Step_ReactionDelay_KeepsTheOldActionUntilItPasses()
        {
            var profile = BotTestSettings.Profile();
            profile.ReactionDelay = 0.5f;
            _brain = new BotBrain(profile, BotTestSettings.Create(), 1, 6);
            _brain.Reset(0f);
            _world.Snow.Refresh(new FakeSnowDepths(3));
            Place(new Vector2(8f, 0f), Vector2.right, 100);

            _brain.Step(_world, Self, 0f);
            Assert.That(_brain.Action, Is.EqualTo(BotAction.Collect));

            _brain.Step(_world, Self, 0.6f);
            Assert.That(_brain.Action, Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Step_FullBucketWhileMistakeProne_StillSwitchesToDeliver()
        {
            var profile = BotTestSettings.Profile(BotDifficulty.Weak);
            profile.MistakeChance = 0.5f;
            profile.ReactionDelay = 0.6f;
            _brain = new BotBrain(profile, BotTestSettings.Create(), 3, 6);
            _brain.Reset(0f);
            _world.Snow.Refresh(new FakeSnowDepths(3));
            Place(new Vector2(-8f, -8f), Vector2.up, 0);
            Run(1f);
            Assume.That(_brain.Action, Is.EqualTo(BotAction.Collect));

            Place(new Vector2(-8f, -8f), Vector2.up, 100);
            Run(3f);

            Assert.That(_brain.Action, Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Step_NoProgress_StartsAnUnstuckManoeuvre()
        {
            _world.Snow.Refresh(new FakeSnowDepths(3));
            Place(new Vector2(-8f, 0f), Vector2.left, 0);

            Run(2f);

            Assert.That(_brain.StuckEvents, Is.GreaterThan(0));
        }

        private void Place(Vector2 position, Vector2 forward, int load)
        {
            _world.ClearVehicles();
            _world.AddVehicle(new BotVehicle(Self, position, Vector2.zero, forward, load));
        }

        private Vector2 Run(float seconds)
        {
            var stick = Vector2.zero;
            for (var time = 0f; time < seconds; time += Tick)
            {
                stick = _brain.Step(_world, Self, _clock + time).Move;
            }

            _clock += seconds;
            return stick;
        }
    }
}
