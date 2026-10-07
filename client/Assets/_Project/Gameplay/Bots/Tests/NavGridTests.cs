using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class NavGridTests
    {
        [Test]
        public void Build_EmptyArena_EveryCellWalkable()
        {
            var grid = BotTestSettings.OpenGrid();

            Assert.That(grid.Width, Is.EqualTo(20));
            Assert.That(grid.Height, Is.EqualTo(20));
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    Assert.That(grid.IsWalkable(new Vector2Int(x, y)), Is.True);
                }
            }
        }

        [Test]
        public void Build_Box_BlocksCellsWithinClearance()
        {
            var grid = BotTestSettings.Grid(VehicleArena.Create().AddBox(Vector2.zero, new Vector2(1f, 1f)));

            Assert.That(grid.IsWalkable(grid.CellOf(new Vector2(0.5f, 0.5f))), Is.False);
            Assert.That(grid.IsWalkable(grid.CellOf(new Vector2(1.5f, 0.5f))), Is.False);
            Assert.That(grid.IsWalkable(grid.CellOf(new Vector2(2.5f, 0.5f))), Is.True);
        }

        [Test]
        public void Build_Circle_BlocksTheInflatedDisc()
        {
            var grid = BotTestSettings.Grid(VehicleArena.Create().AddCircle(new Vector2(0.5f, 0.5f), 1.5f));

            Assert.That(grid.IsWalkable(grid.CellOf(new Vector2(2.5f, 0.5f))), Is.False);
            Assert.That(grid.IsWalkable(grid.CellOf(new Vector2(3.5f, 0.5f))), Is.True);
        }

        [Test]
        public void CellOf_PointOutsideGrid_IsClampedInside()
        {
            var grid = BotTestSettings.OpenGrid();

            Assert.That(grid.CellOf(new Vector2(-50f, 50f)), Is.EqualTo(new Vector2Int(0, 19)));
        }

        [Test]
        public void CentreOf_Cell_IsTheMiddleOfItsSquare()
        {
            var grid = BotTestSettings.OpenGrid();

            Assert.That(grid.CentreOf(new Vector2Int(10, 10)), Is.EqualTo(new Vector2(0.5f, 0.5f)));
        }

        [Test]
        public void NearestWalkable_FromBlockedCell_FindsTheClosestOpenOne()
        {
            var grid = BotTestSettings.Grid(VehicleArena.Create().AddCircle(new Vector2(0.5f, 0.5f), 1.5f));

            var nearest = grid.NearestWalkable(grid.CellOf(new Vector2(1.5f, 0.5f)));

            Assert.That(grid.IsWalkable(nearest), Is.True);
            Assert.That(Vector2.Distance(grid.CentreOf(nearest), new Vector2(0.5f, 0.5f)), Is.LessThan(3.6f));
        }

        [Test]
        public void HasLineOfSight_AcrossAWall_IsFalse()
        {
            var grid = BotTestSettings.Grid(VehicleArena.Create().AddBox(Vector2.zero, new Vector2(0.5f, 6f)));

            Assert.That(grid.HasLineOfSight(new Vector2(-5f, 0f), new Vector2(5f, 0f)), Is.False);
            Assert.That(grid.HasLineOfSight(new Vector2(-5f, 8.5f), new Vector2(5f, 8.5f)), Is.True);
        }
    }
}
