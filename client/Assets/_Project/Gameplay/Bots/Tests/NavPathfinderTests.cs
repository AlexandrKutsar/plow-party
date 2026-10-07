using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Gameplay.Bots.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class NavPathfinderTests
    {
        private readonly List<Vector2> _path = new List<Vector2>();

        [Test]
        public void TryFindPath_OpenField_IsOneStraightLeg()
        {
            var pathfinder = new NavPathfinder(BotTestSettings.OpenGrid());

            Assert.That(pathfinder.TryFindPath(new Vector2(-8f, -8f), new Vector2(8f, 6f), _path), Is.True);
            Assert.That(_path.Count, Is.EqualTo(1));
            Assert.That(_path[0], Is.EqualTo(new Vector2(8f, 6f)));
        }

        [Test]
        public void TryFindPath_WallInTheWay_EveryLegStaysClear()
        {
            var grid = BotTestSettings.Grid(VehicleArena.Create().AddBox(Vector2.zero, new Vector2(0.5f, 6f)));
            var pathfinder = new NavPathfinder(grid);
            var from = new Vector2(-5f, 0f);

            Assert.That(pathfinder.TryFindPath(from, new Vector2(5f, 0f), _path), Is.True);
            Assert.That(_path.Count, Is.GreaterThan(1));
            var previous = from;
            foreach (var waypoint in _path)
            {
                Assert.That(grid.HasLineOfSight(previous, waypoint), Is.True, $"{previous} -> {waypoint}");
                previous = waypoint;
            }
        }

        [Test]
        public void TryFindPath_GoalWalledIn_ReturnsFalse()
        {
            var arena = VehicleArena.Create()
                .AddBox(new Vector2(5f, 2.5f), new Vector2(3f, 0.5f))
                .AddBox(new Vector2(5f, 7.5f), new Vector2(3f, 0.5f))
                .AddBox(new Vector2(2.5f, 5f), new Vector2(0.5f, 3f))
                .AddBox(new Vector2(7.5f, 5f), new Vector2(0.5f, 3f));
            var pathfinder = new NavPathfinder(BotTestSettings.Grid(arena));

            Assert.That(pathfinder.TryFindPath(new Vector2(-8f, -8f), new Vector2(5f, 5f), _path), Is.False);
        }

        [Test]
        public void TryFindPath_RingWithOneGap_EntersThroughTheGap()
        {
            var arena = VehicleArena.Create();
            for (var degrees = 30; degrees < 360; degrees += 20)
            {
                var angle = degrees * Mathf.Deg2Rad;
                arena.AddCircle(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 6f, 0.8f);
            }

            var grid = BotTestSettings.Grid(arena);
            var pathfinder = new NavPathfinder(grid);
            var from = new Vector2(-9f, 0f);

            Assert.That(pathfinder.TryFindPath(from, Vector2.zero, _path), Is.True);
            var previous = from;
            foreach (var waypoint in _path)
            {
                Assert.That(grid.HasLineOfSight(previous, waypoint), Is.True, $"{previous} -> {waypoint}");
                previous = waypoint;
            }
        }

        [Test]
        public void TryFindPath_StartInsideClearance_StillFindsAWay()
        {
            var arena = VehicleArena.Create().AddCircle(Vector2.zero, 1f);
            var pathfinder = new NavPathfinder(BotTestSettings.Grid(arena));

            Assert.That(pathfinder.TryFindPath(new Vector2(1.3f, 0f), new Vector2(-6f, 0f), _path), Is.True);
            Assert.That(_path[_path.Count - 1], Is.EqualTo(new Vector2(-6f, 0f)));
        }
    }
}
