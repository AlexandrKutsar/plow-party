using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class VehicleMovementTests
    {
        private VehicleWorld _world;

        [SetUp]
        public void SetUp()
        {
            _world = new VehicleWorld(VehicleTestSettings.Create(), VehicleArena.Create(), 6);
        }

        [Test]
        public void Tick_StickAlongForward_AcceleratesAlongForward()
        {
            var vehicle = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.SetControl(vehicle, VehicleInput.Stick(Vector2.up), VehicleModifiers.None);

            _world.Tick(0.1f);

            var state = _world.GetVehicle(vehicle);
            Assert.That(state.Velocity.y, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(state.Velocity.x, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(state.Position.y, Is.EqualTo(0.2f).Within(1e-4f));
        }

        [Test]
        public void Tick_StickPerpendicular_TurnsForwardByTurnRateOnly()
        {
            var vehicle = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.SetControl(vehicle, VehicleInput.Stick(Vector2.right), VehicleModifiers.None);

            _world.Tick(0.25f);

            var forward = _world.GetVehicle(vehicle).Forward;
            Assert.That(Vector2.Angle(Vector2.up, forward), Is.EqualTo(45f).Within(0.01f));
            Assert.That(Vector2.SignedAngle(Vector2.up, forward), Is.LessThan(0f));
        }

        [Test]
        public void Tick_StickReleased_CoastsDownByDeceleration()
        {
            var vehicle = _world.Add(new VehicleState(Vector2.zero, Vector2.up * 10f, Vector2.up));

            _world.Tick(0.5f);

            Assert.That(_world.GetVehicle(vehicle).Velocity.magnitude, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void Tick_StickReleasedLongEnough_ComesToRest()
        {
            var vehicle = _world.Add(new VehicleState(Vector2.zero, Vector2.up * 10f, Vector2.up));

            TickFor(2f);

            Assert.That(_world.GetVehicle(vehicle).Velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Tick_HalfStickHeld_SettlesAtHalfMaxSpeed()
        {
            var vehicle = _world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            _world.SetControl(vehicle, VehicleInput.Stick(Vector2.up * 0.5f), VehicleModifiers.None);

            TickFor(2f);

            Assert.That(_world.GetVehicle(vehicle).Velocity.magnitude, Is.EqualTo(5f).Within(1e-4f));
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
