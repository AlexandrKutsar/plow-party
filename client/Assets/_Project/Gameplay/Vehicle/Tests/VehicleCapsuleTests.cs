using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class VehicleCapsuleTests
    {
        private const float Step = 0.02f;

        [Test]
        public void Tick_VehiclesNoseToTailOverlapping_SeparateToFullLength()
        {
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), VehicleArena.Create(), 6);
            var rear = world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            var front = world.Add(VehicleState.At(new Vector2(0f, 1.5f), Vector2.up));

            world.Tick(Step);

            Assert.That(world.GetVehicle(front).Position.y - world.GetVehicle(rear).Position.y, Is.EqualTo(1.6f).Within(1e-4f));
        }

        [Test]
        public void Tick_NoseInsideBoxWall_PushedBackUntilNoseTouches()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(1.5f, 0f), new Vector2(0.5f, 5f));
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), arena, 6);
            var vehicle = world.Add(VehicleState.At(new Vector2(0.3f, 0f), Vector2.right));

            world.Tick(Step);

            Assert.That(world.GetVehicle(vehicle).Position.x, Is.EqualTo(0.2f).Within(1e-4f));
        }

        [Test]
        public void Tick_ParkedAlongsideBoxWall_StaysPut()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(1.5f, 0f), new Vector2(0.5f, 5f));
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), arena, 6);
            var vehicle = world.Add(VehicleState.At(new Vector2(0.45f, 0f), Vector2.up));

            world.Tick(Step);

            Assert.That(world.GetVehicle(vehicle).Position, Is.EqualTo(new Vector2(0.45f, 0f)));
        }

        [Test]
        public void Tick_BoxCornerInsideFlank_PushedStraightAwayFromAxis()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(0.7f, -0.85f), new Vector2(0.5f, 0.5f));
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), arena, 6);
            var vehicle = world.Add(VehicleState.At(Vector2.zero, Vector2.right));

            world.Tick(Step);

            var position = world.GetVehicle(vehicle).Position;
            Assert.That(position.x, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(position.y, Is.EqualTo(0.15f).Within(1e-4f));
        }

        [Test]
        public void Tick_TailInsideCircleObstacle_PushedForward()
        {
            var arena = VehicleArena.Create().AddCircle(new Vector2(0f, -1f), 0.3f);
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), arena, 6);
            var vehicle = world.Add(VehicleState.At(Vector2.zero, Vector2.up));

            world.Tick(Step);

            Assert.That(world.GetVehicle(vehicle).Position.y, Is.EqualTo(0.1f).Within(1e-4f));
        }

        [Test]
        public void Tick_TurningToFaceWallFromAlongside_NoseNeverEntersWall()
        {
            var settings = VehicleTestSettings.CreateCapsule();
            var arena = VehicleArena.Create().AddBox(new Vector2(1.5f, 0f), new Vector2(0.5f, 5f));
            var world = new VehicleWorld(settings, arena, 6);
            var vehicle = world.Add(VehicleState.At(new Vector2(0.45f, 0f), Vector2.up));
            world.SetControl(vehicle, VehicleInput.Stick(Vector2.right * 0.1f), VehicleModifiers.None);

            for (var tick = 0; tick < 60; tick++)
            {
                world.Tick(VehicleTestSettings.Tick);
                var state = world.GetVehicle(vehicle);
                var nose = state.Position.x + Mathf.Abs(state.Forward.x) * settings.HalfLength + settings.Radius;
                Assert.That(nose, Is.LessThanOrEqualTo(1f + 1e-3f), $"tick {tick}");
            }
        }

        [Test]
        public void Tick_HitIntoLongSideOffCentre_ReportsRam()
        {
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), VehicleArena.Create(), 6);
            var victim = world.Add(VehicleState.At(Vector2.zero, Vector2.right));
            var rammer = world.Add(new VehicleState(new Vector2(0.3f, -1.15f), Vector2.up * 8f, Vector2.up));

            world.Tick(Step);

            Assert.That(world.Rams.Count, Is.EqualTo(1));
            Assert.That(world.Rams[0].Rammer, Is.EqualTo(rammer));
            Assert.That(world.Rams[0].Victim, Is.EqualTo(victim));
        }

        [Test]
        public void Tick_HitIntoTail_ReportsRam()
        {
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), VehicleArena.Create(), 6);
            var victim = world.Add(VehicleState.At(Vector2.zero, Vector2.up));
            world.Add(new VehicleState(new Vector2(0f, -1.7f), Vector2.up * 8f, Vector2.up));

            world.Tick(Step);

            Assert.That(world.Rams.Count, Is.EqualTo(1));
            Assert.That(world.Rams[0].Victim, Is.EqualTo(victim));
        }

        [Test]
        public void Tick_HitIntoNose_BouncesWithoutRam()
        {
            var world = new VehicleWorld(VehicleTestSettings.CreateCapsule(), VehicleArena.Create(), 6);
            var victim = world.Add(VehicleState.At(Vector2.zero, Vector2.left));
            world.Add(new VehicleState(new Vector2(-1.7f, 0f), Vector2.right * 8f, Vector2.right));

            world.Tick(Step);

            Assert.That(world.Rams, Is.Empty);
            Assert.That(world.GetVehicle(victim).Velocity.x, Is.GreaterThan(0f));
        }
    }
}
