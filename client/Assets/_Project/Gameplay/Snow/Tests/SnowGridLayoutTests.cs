using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridLayoutTests
    {
        [Test]
        public void Constructor_EmptyArena_EveryCellAtFullDepth()
        {
            var grid = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);

            Assert.That(grid.Width, Is.EqualTo(8));
            Assert.That(grid.Height, Is.EqualTo(4));
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    Assert.That(grid.GetDepth(x, y), Is.EqualTo(3));
                }
            }
        }

        [Test]
        public void Constructor_BoxObstacle_CellsWithCentreInsideAreEmpty()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));

            var grid = new SnowGrid(SnowTestSettings.Create(), arena, SnowTestSettings.Seed);

            Assert.That(grid.GetDepth(1, 1), Is.EqualTo(0));
            Assert.That(grid.GetDepth(2, 2), Is.EqualTo(0));
            Assert.That(grid.GetDepth(0, 1), Is.EqualTo(3));
            Assert.That(grid.GetDepth(3, 1), Is.EqualTo(3));
            Assert.That(grid.GetDepth(1, 3), Is.EqualTo(3));
        }

        [Test]
        public void Constructor_CircleObstacle_CellsWithCentreInsideAreEmpty()
        {
            var arena = VehicleArena.Create().AddCircle(new Vector2(3f, 1f), 0.4f);

            var grid = new SnowGrid(SnowTestSettings.Create(), arena, SnowTestSettings.Seed);

            Assert.That(grid.GetDepth(5, 1), Is.EqualTo(0));
            Assert.That(grid.GetDepth(6, 2), Is.EqualTo(0));
            Assert.That(grid.GetDepth(4, 1), Is.EqualTo(3));
            Assert.That(grid.GetDepth(5, 0), Is.EqualTo(3));
        }

        [Test]
        public void Constructor_GridOffsetFromWorldOrigin_ObstacleMaskFollowsOrigin()
        {
            var settings = SnowTestSettings.Create();
            settings.Origin = new Vector2(-2f, -1f);
            var arena = VehicleArena.Create().AddBox(new Vector2(-1.75f, -0.75f), new Vector2(0.1f, 0.1f));

            var grid = new SnowGrid(settings, arena, SnowTestSettings.Seed);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
            Assert.That(grid.GetDepth(1, 0), Is.EqualTo(3));
        }
    }
}
