using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class VehicleRamTests
    {
        private const float Step = 0.02f;

        private VehicleWorld _world;

        [SetUp]
        public void SetUp()
        {
            _world = new VehicleWorld(VehicleTestSettings.Create(), VehicleArena.Create(), 6);
        }

        [Test]
        public void Tick_HitIntoVictimSide_ReportsRamFromRammerToVictim()
        {
            var victim = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            var rammer = _world.Add(new VehicleState(new Vector2(-0.95f, 0f), Vector2.right * 8f, Vector2.right));

            _world.Tick(Step);

            Assert.That(_world.Rams.Count, Is.EqualTo(1));
            Assert.That(_world.Rams[0].Rammer, Is.EqualTo(rammer));
            Assert.That(_world.Rams[0].Victim, Is.EqualTo(victim));
        }

        [Test]
        public void Tick_HitIntoVictimRear_ReportsRam()
        {
            var victim = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.Add(new VehicleState(new Vector2(0f, -0.95f), Vector2.up * 8f, Vector2.up));

            _world.Tick(Step);

            Assert.That(_world.Rams.Count, Is.EqualTo(1));
            Assert.That(_world.Rams[0].Victim, Is.EqualTo(victim));
        }

        [Test]
        public void Tick_HeadOnHit_ReportsNoRam()
        {
            _world.Add(VehicleState.At(Vector2.zero, Vector2.left));
            _world.Add(new VehicleState(new Vector2(-0.95f, 0f), Vector2.right * 8f, Vector2.right));

            _world.Tick(Step);

            Assert.That(_world.Rams, Is.Empty);
        }

        [Test]
        public void Tick_SlowTouchIntoSide_ReportsNoRam()
        {
            _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.Add(new VehicleState(new Vector2(-0.99f, 0f), Vector2.right * 2f, Vector2.right));

            _world.Tick(Step);

            Assert.That(_world.Rams, Is.Empty);
        }

        [Test]
        public void Tick_SamePairHitsAgainWithinCooldown_ReportsNoSecondRam()
        {
            var victim = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            var rammer = _world.Add(new VehicleState(new Vector2(-0.95f, 0f), Vector2.right * 8f, Vector2.right));
            _world.Tick(Step);

            TickFor(0.5f);
            Replay(victim, rammer);

            Assert.That(_world.Rams, Is.Empty);
        }

        [Test]
        public void Tick_SamePairHitsAgainAfterCooldown_ReportsRamAgain()
        {
            var victim = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            var rammer = _world.Add(new VehicleState(new Vector2(-0.95f, 0f), Vector2.right * 8f, Vector2.right));
            _world.Tick(Step);

            TickFor(1.1f);
            Replay(victim, rammer);

            Assert.That(_world.Rams.Count, Is.EqualTo(1));
        }

        [Test]
        public void Tick_Ram_RammerBouncesBack()
        {
            _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            var rammer = _world.Add(new VehicleState(new Vector2(-0.95f, 0f), Vector2.right * 8f, Vector2.right));

            _world.Tick(Step);

            Assert.That(_world.GetVehicle(rammer).Velocity.x, Is.LessThan(0f));
        }

        [Test]
        public void Tick_RamWithStrengthMultiplier_HitsVictimHarderAndReportsStrongerRam()
        {
            var plain = new VehicleWorld(VehicleTestSettings.Create(), VehicleArena.Create(), 6);
            var boosted = new VehicleWorld(VehicleTestSettings.Create(), VehicleArena.Create(), 6);
            foreach (var world in new[] { plain, boosted })
            {
                world.Add(VehicleState.At(Vector2.zero, Vector2.up));
                world.Add(new VehicleState(new Vector2(-0.95f, 0f), Vector2.right * 8f, Vector2.right));
            }

            boosted.SetControl(1, VehicleInput.Idle, new VehicleModifiers(1f, false, Vector2.zero, 2f));
            plain.Tick(Step);
            boosted.Tick(Step);

            Assert.That(boosted.GetVehicle(0).Velocity.x, Is.GreaterThan(plain.GetVehicle(0).Velocity.x));
            Assert.That(boosted.Rams[0].Strength, Is.EqualTo(plain.Rams[0].Strength * 2f).Within(1e-4f));
        }

        [Test]
        public void Tick_SameInputsInTwoWorlds_ProducesIdenticalResults()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(0f, 6f), new Vector2(6f, 0.5f)).AddCircle(new Vector2(2f, 2f), 1f);
            var first = new VehicleWorld(VehicleTestSettings.Create(), arena, 6);
            var second = new VehicleWorld(VehicleTestSettings.Create(), arena, 6);
            foreach (var world in new[] { first, second })
            {
                world.Add(VehicleState.At(new Vector2(-2f, 0f), Vector2.right));
                world.Add(VehicleState.At(new Vector2(2f, -1f), Vector2.left));
                world.Add(VehicleState.At(new Vector2(0f, -3f), Vector2.up));
            }

            for (var tick = 0; tick < 600; tick++)
            {
                var angle = tick * 0.05f;
                foreach (var world in new[] { first, second })
                {
                    world.SetControl(0, VehicleInput.Stick(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))), VehicleModifiers.None);
                    world.SetControl(1, VehicleInput.Stick(new Vector2(-1f, Mathf.Sin(angle))), VehicleModifiers.None);
                    world.SetControl(2, VehicleInput.Stick(Vector2.up), VehicleModifiers.None);
                    world.Tick(VehicleTestSettings.Tick);
                }
            }

            for (var i = 0; i < 3; i++)
            {
                Assert.That(second.GetVehicle(i).Position, Is.EqualTo(first.GetVehicle(i).Position));
                Assert.That(second.GetVehicle(i).Velocity, Is.EqualTo(first.GetVehicle(i).Velocity));
                Assert.That(second.GetVehicle(i).Forward, Is.EqualTo(first.GetVehicle(i).Forward));
            }
        }

        private void Replay(int victim, int rammer)
        {
            _world.SetVehicle(victim, VehicleState.At(Vector2.zero, Vector2.up));
            _world.SetVehicle(rammer, new VehicleState(new Vector2(-0.95f, 0f), Vector2.right * 8f, Vector2.right));
            _world.Tick(Step);
        }

        private void TickFor(float seconds)
        {
            for (var elapsed = 0f; elapsed < seconds; elapsed += VehicleTestSettings.Tick)
            {
                _world.Tick(VehicleTestSettings.Tick);
            }
        }
    }
}
