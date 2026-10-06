using NUnit.Framework;
using PlowParty.Gameplay.DropOff.Simulation;

namespace PlowParty.Gameplay.DropOff.Tests
{
    public sealed class DropOffRulesTickTests
    {
        private const float TickDelta = 1f / 60f;
        private const int FullBucket = DropOffTestSettings.FullBucketSteps;

        private DropOffRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = DropOffTestSettings.CreateRules();
        }

        [Test]
        public void Tick_OutsideZone_EndsDeliveryWithoutUnloading()
        {
            var tick = _rules.Tick(new Delivery(2f, 10, 0.1f), false, 100, TickDelta);

            Assert.That(tick.Next.IsActive, Is.False);
            Assert.That(tick.UnloadedSteps, Is.EqualTo(0));
            Assert.That(tick.ScoreGained, Is.EqualTo(0));
        }

        [Test]
        public void Tick_EnteringWithEmptyBucket_StartsNoDelivery()
        {
            var tick = _rules.Tick(Delivery.None, true, 0, TickDelta);

            Assert.That(tick.Next.IsActive, Is.False);
            Assert.That(tick.UnloadedSteps, Is.EqualTo(0));
        }

        [Test]
        public void Tick_EnteringWithLoad_LocksMultiplierByThatLoad()
        {
            var tick = _rules.Tick(Delivery.None, true, 120, 0.1f);

            Assert.That(tick.Next.Multiplier, Is.EqualTo(1.5f));
        }

        [Test]
        public void Tick_LoadFallsDuringDelivery_KeepsLockedMultiplier()
        {
            var tick = _rules.Tick(new Delivery(2f, 100, 1f), true, 100, 0.1f);

            Assert.That(tick.Next.Multiplier, Is.EqualTo(2f));
        }

        [Test]
        public void Tick_HalfSecondInZone_UnloadsAtConfiguredRate()
        {
            var tick = _rules.Tick(Delivery.None, true, FullBucket, 0.5f);

            Assert.That(tick.UnloadedSteps, Is.EqualTo(50));
        }

        [Test]
        public void Tick_FullBucketForFullDuration_UnloadsEverythingForDoubleScore()
        {
            var result = Deliver(FullBucket, TickDelta, 1000);

            Assert.That(result.Unloaded, Is.EqualTo(FullBucket));
            Assert.That(result.Score, Is.EqualTo(200));
            Assert.That(result.Ticks, Is.InRange(120, 121));
        }

        [Test]
        public void Tick_LoadRunsOut_EndsDelivery()
        {
            var tick = _rules.Tick(new Delivery(1f, 0, 0f), true, 3, 0.5f);

            Assert.That(tick.UnloadedSteps, Is.EqualTo(3));
            Assert.That(tick.Next.IsActive, Is.False);
        }

        [Test]
        public void Tick_LeavingMidway_KeepsScoreForUnloadedPartAtLockedMultiplier()
        {
            var inside = _rules.Tick(Delivery.None, true, FullBucket, 0.5f);
            var outside = _rules.Tick(inside.Next, false, FullBucket - inside.UnloadedSteps, 0.5f);

            Assert.That(inside.ScoreGained + outside.ScoreGained, Is.EqualTo(50));
            Assert.That(outside.Next.IsActive, Is.False);
        }

        [Test]
        public void Tick_FractionalScore_RoundsDownOncePerDelivery()
        {
            var result = Deliver(51 * DropOffTestSettings.StepsPerLoad, 0.05f, 1000);

            Assert.That(result.Score, Is.EqualTo(76));
        }

        [Test]
        public void Tick_ZeroDuration_UnloadsEverythingAtOnce()
        {
            var settings = DropOffTestSettings.Create();
            settings.FullUnloadDuration = 0f;
            var rules = new DropOffRules(settings, DropOffTestSettings.CreateBucket());

            var tick = rules.Tick(Delivery.None, true, FullBucket, TickDelta);

            Assert.That(tick.UnloadedSteps, Is.EqualTo(FullBucket));
            Assert.That(tick.ScoreGained, Is.EqualTo(200));
        }

        [Test]
        public void Tick_ClearedDeliveryStillInZone_RelocksByRemainingLoad()
        {
            var tick = _rules.Tick(Delivery.None, true, 60, TickDelta);

            Assert.That(tick.Next.Multiplier, Is.EqualTo(1f));
        }

        private DeliveryResult Deliver(int loadSteps, float deltaTime, int maxTicks)
        {
            var result = new DeliveryResult();
            var delivery = Delivery.None;
            var load = loadSteps;
            while (load > 0 && result.Ticks < maxTicks)
            {
                var tick = _rules.Tick(delivery, true, load, deltaTime);
                load -= tick.UnloadedSteps;
                result.Unloaded += tick.UnloadedSteps;
                result.Score += tick.ScoreGained;
                result.Ticks++;
                delivery = tick.Next;
            }

            return result;
        }

        private sealed class DeliveryResult
        {
            public int Unloaded { get; set; }

            public int Score { get; set; }

            public int Ticks { get; set; }
        }
    }
}
