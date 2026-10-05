using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class VehicleCollisionTests
    {
        private const float Step = 0.02f;

        [Test]
        public void Tick_DrivingIntoBoxWall_StopsAtWallAndBouncesBack()
        {
            var arena = VehicleArena.Empty.AddBox(new Vector2(1.5f, 0f), new Vector2(0.5f, 5f));
            var world = new VehicleWorld(VehicleTestSettings.Create(), arena, 6);
            var vehicle = world.Add(new VehicleState(new Vector2(0.4f, 0f), Vector2.right * 10f, Vector2.right));

            world.Tick(Step);

            var state = world.GetVehicle(vehicle);
            Assert.That(state.Position.x, Is.EqualTo(0.5f).Within(1e-4f));
            Assert.That(state.Velocity.x, Is.EqualTo(-7.84f).Within(1e-4f));
        }

        [Test]
        public void Tick_SlidingAlongBoxWall_KeepsTangentialSpeed()
        {
            var arena = VehicleArena.Empty.AddBox(new Vector2(1.5f, 0f), new Vector2(0.5f, 5f));
            var world = new VehicleWorld(VehicleTestSettings.Create(), arena, 6);
            var vehicle = world.Add(new VehicleState(new Vector2(0.5f, 0f), new Vector2(1f, 5f), Vector2.up));
            world.SetControl(vehicle, VehicleInput.Stick(Vector2.up), VehicleModifiers.None);

            world.Tick(Step);

            var state = world.GetVehicle(vehicle);
            Assert.That(state.Position.x, Is.LessThanOrEqualTo(0.5f + 1e-4f));
            Assert.That(state.Velocity.y, Is.GreaterThan(5f));
        }

        [Test]
        public void Tick_DrivingIntoCircleObstacle_StopsAtSurfaceAndBouncesBack()
        {
            var arena = VehicleArena.Empty.AddCircle(new Vector2(2f, 0f), 1f);
            var world = new VehicleWorld(VehicleTestSettings.Create(), arena, 6);
            var vehicle = world.Add(new VehicleState(new Vector2(0.4f, 0f), Vector2.right * 10f, Vector2.right));

            world.Tick(Step);

            var state = world.GetVehicle(vehicle);
            Assert.That(state.Position.x, Is.EqualTo(0.5f).Within(1e-4f));
            Assert.That(state.Velocity.x, Is.EqualTo(-7.84f).Within(1e-4f));
        }

        [Test]
        public void Tick_TwoVehiclesCollide_SeparateAndBounceApart()
        {
            var world = new VehicleWorld(VehicleTestSettings.Create(), VehicleArena.Empty, 6);
            var left = world.Add(new VehicleState(new Vector2(-0.45f, 0f), Vector2.right * 5f, Vector2.right));
            var right = world.Add(new VehicleState(new Vector2(0.45f, 0f), Vector2.left * 5f, Vector2.left));

            world.Tick(Step);

            var a = world.GetVehicle(left);
            var b = world.GetVehicle(right);
            Assert.That(Vector2.Distance(a.Position, b.Position), Is.EqualTo(1f).Within(1e-4f));
            Assert.That(a.Velocity.x, Is.EqualTo(-3.84f).Within(1e-4f));
            Assert.That(b.Velocity.x, Is.EqualTo(3.84f).Within(1e-4f));
        }

        [Test]
        public void Tick_VehiclesMovingApartWhileTouching_KeepTheirVelocities()
        {
            var world = new VehicleWorld(VehicleTestSettings.Create(), VehicleArena.Empty, 6);
            var left = world.Add(new VehicleState(new Vector2(-0.45f, 0f), Vector2.left * 5f, Vector2.left));
            var right = world.Add(new VehicleState(new Vector2(0.45f, 0f), Vector2.right * 5f, Vector2.right));

            world.Tick(Step);

            Assert.That(world.GetVehicle(left).Velocity.x, Is.EqualTo(-4.8f).Within(1e-4f));
            Assert.That(world.GetVehicle(right).Velocity.x, Is.EqualTo(4.8f).Within(1e-4f));
        }

        [Test]
        public void Tick_VehiclePushedIntoWallByAnother_EndsOutsideWall()
        {
            var arena = VehicleArena.Empty.AddBox(new Vector2(1.5f, 0f), new Vector2(0.5f, 5f));
            var world = new VehicleWorld(VehicleTestSettings.Create(), arena, 6);
            var pinned = world.Add(new VehicleState(new Vector2(0.5f, 0f), Vector2.zero, Vector2.up));
            world.Add(new VehicleState(new Vector2(-0.4f, 0f), Vector2.right * 10f, Vector2.right));

            world.Tick(Step);

            Assert.That(world.GetVehicle(pinned).Position.x, Is.LessThanOrEqualTo(0.5f + 1e-4f));
        }
    }
}
