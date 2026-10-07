using NUnit.Framework;
using PlowParty.Gameplay.Bots.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class BotSteeringTests
    {
        private const int Self = 0;
        private const int NoIgnored = -1;

        private readonly BotSettings _settings = BotTestSettings.Create();

        [Test]
        public void Steer_ClearRoad_PointsAtTheAimAtFullThrottle()
        {
            var vehicles = new[] { Vehicle(Self, Vector2.zero, Vector2.right) };

            var stick = BotSteering.Steer(vehicles[0], new Vector2(10f, 0f), vehicles, 1, NoIgnored, _settings, 0f, 1f);

            Assert.That(stick.x, Is.EqualTo(1f).Within(1e-4f));
            Assert.That(stick.y, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Steer_VehicleRightAhead_TurnsAside()
        {
            var vehicles = new[] { Vehicle(Self, Vector2.zero, Vector2.right), Vehicle(1, new Vector2(1.5f, 0.3f), Vector2.up) };

            var stick = BotSteering.Steer(vehicles[0], new Vector2(10f, 0f), vehicles, 2, NoIgnored, _settings, 0f, 1f);

            Assert.That(stick.y, Is.LessThan(-0.1f));
        }

        [Test]
        public void Steer_VehicleAheadIsTheRamTarget_DrivesStraightAtIt()
        {
            var vehicles = new[] { Vehicle(Self, Vector2.zero, Vector2.right), Vehicle(1, new Vector2(1.5f, 0f), Vector2.up) };

            var stick = BotSteering.Steer(vehicles[0], new Vector2(1.5f, 0f), vehicles, 2, 1, _settings, 0f, 1f);

            Assert.That(stick.normalized.x, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void Steer_AimBehind_SlowsDownToTurn()
        {
            var vehicles = new[] { Vehicle(Self, Vector2.zero, Vector2.right) };

            var stick = BotSteering.Steer(vehicles[0], new Vector2(-10f, 0.5f), vehicles, 1, NoIgnored, _settings, 0f, 1f);

            Assert.That(stick.magnitude, Is.LessThan(0.5f));
            Assert.That(stick.x, Is.LessThan(0f));
        }

        [Test]
        public void Steer_ThrottleCap_LimitsTheStick()
        {
            var vehicles = new[] { Vehicle(Self, Vector2.zero, Vector2.right) };

            var stick = BotSteering.Steer(vehicles[0], new Vector2(10f, 0f), vehicles, 1, NoIgnored, _settings, 0f, 0.8f);

            Assert.That(stick.magnitude, Is.EqualTo(0.8f).Within(1e-4f));
        }

        [Test]
        public void Steer_Noise_TurnsTheStickByThatAngle()
        {
            var vehicles = new[] { Vehicle(Self, Vector2.zero, Vector2.right) };

            var stick = BotSteering.Steer(vehicles[0], new Vector2(10f, 0f), vehicles, 1, NoIgnored, _settings, 10f, 1f);

            Assert.That(Vector2.SignedAngle(Vector2.right, stick), Is.EqualTo(10f).Within(0.01f));
        }

        private static BotVehicle Vehicle(int slot, Vector2 position, Vector2 forward)
        {
            return new BotVehicle(slot, position, Vector2.zero, forward, 0);
        }
    }
}
