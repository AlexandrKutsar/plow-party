using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class VehicleModifierTests
    {
        private VehicleWorld _world;

        [SetUp]
        public void SetUp()
        {
            _world = new VehicleWorld(VehicleTestSettings.Create(), VehicleArena.Empty, 6);
        }

        [Test]
        public void Tick_SpeedMultiplierBelowOne_SettlesAtReducedTopSpeed()
        {
            var vehicle = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.SetControl(vehicle, VehicleInput.Stick(Vector2.up), new VehicleModifiers(0.75f, false, Vector2.zero, 1f));

            TickFor(2f);

            Assert.That(_world.GetVehicle(vehicle).Velocity.magnitude, Is.EqualTo(7.5f).Within(1e-4f));
        }

        [Test]
        public void Tick_SpeedMultiplierAboveOne_ExceedsMaxSpeed()
        {
            var vehicle = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.SetControl(vehicle, VehicleInput.Stick(Vector2.up), new VehicleModifiers(1.5f, false, Vector2.zero, 1f));

            TickFor(2f);

            Assert.That(_world.GetVehicle(vehicle).Velocity.magnitude, Is.EqualTo(15f).Within(1e-4f));
        }

        [Test]
        public void Tick_Immobilised_StaysInPlaceDespiteStick()
        {
            var vehicle = _world.Add(new VehicleState(new Vector2(3f, 4f), Vector2.up * 10f, Vector2.up));
            _world.SetControl(vehicle, VehicleInput.Stick(Vector2.right), new VehicleModifiers(1f, true, Vector2.zero, 1f));

            TickFor(1f);

            var state = _world.GetVehicle(vehicle);
            Assert.That(state.Position, Is.EqualTo(new Vector2(3f, 4f)));
            Assert.That(state.Velocity, Is.EqualTo(Vector2.zero));
            Assert.That(state.Forward, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void Tick_Impulse_AddsToVelocityOnce()
        {
            var vehicle = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.SetControl(vehicle, VehicleInput.Idle, new VehicleModifiers(1f, false, Vector2.right * 6f, 1f));

            _world.Tick(0.1f);

            Assert.That(_world.GetVehicle(vehicle).Velocity.x, Is.EqualTo(5f).Within(1e-4f));

            _world.Tick(0.1f);

            Assert.That(_world.GetVehicle(vehicle).Velocity.x, Is.EqualTo(4f).Within(1e-4f));
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
