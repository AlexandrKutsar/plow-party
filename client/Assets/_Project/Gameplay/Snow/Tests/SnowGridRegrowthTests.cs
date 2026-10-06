using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridRegrowthTests
    {
        private static readonly SnowBlade WholeGrid = new SnowBlade(new Vector2(2f, 1f), Vector2.up, 4.1f, 2.1f);

        [Test]
        public void Tick_BeforeFirstInterval_LeavesClearedCells()
        {
            var grid = CreateCleared(1f, SnowTestSettings.Seed);

            grid.Tick(0.99f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_CertainChance_RaisesClearedCellOneStepPerInterval()
        {
            var grid = CreateCleared(1f, SnowTestSettings.Seed);

            grid.Tick(1f);
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(1));

            grid.Tick(2f);
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(2));
        }

        [Test]
        public void Tick_CertainChanceForLong_StopsAtFullDepth()
        {
            var grid = CreateCleared(1f, SnowTestSettings.Seed);

            grid.Tick(10f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(3));
        }

        [Test]
        public void Tick_CertainChance_NeverTouchesSnowPile()
        {
            var grid = CreateCleared(1f, SnowTestSettings.Seed);
            grid.Spill(new Vector2(0.25f, 0.25f), 10);

            grid.Tick(5f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(10));
        }

        [Test]
        public void Tick_CertainChance_NeverFillsObstacleCells()
        {
            var settings = SnowTestSettings.Create();
            settings.RegrowthChance = 1f;
            var grid = new SnowGrid(settings, VehicleArena.Create().AddBox(new Vector2(0.25f, 0.25f), new Vector2(0.1f, 0.1f)), SnowTestSettings.Seed);

            grid.Tick(5f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_HalfChance_RegrowsSomeCellsButNotAll()
        {
            var grid = CreateCleared(0.5f, SnowTestSettings.Seed);

            grid.Tick(1f);

            var regrown = CountAtDepth(grid, 1);
            Assert.That(regrown, Is.GreaterThan(4));
            Assert.That(regrown, Is.LessThan(28));
            Assert.That(CountAtDepth(grid, 0), Is.EqualTo(32 - regrown));
        }

        [Test]
        public void Tick_SameTimeReachedInSmallSteps_GivesSameGridAsOneStep()
        {
            var oneStep = CreateCleared(0.5f, SnowTestSettings.Seed);
            var smallSteps = CreateCleared(0.5f, SnowTestSettings.Seed);

            oneStep.Tick(5f);
            for (var tick = 1; tick <= 50; tick++)
            {
                smallSteps.Tick(tick * 0.1f);
            }

            AssertSameDepths(oneStep, smallSteps);
        }

        [Test]
        public void Tick_TimeGoingBackwards_AppliesNothingTwice()
        {
            var grid = CreateCleared(1f, SnowTestSettings.Seed);
            grid.Tick(2f);

            grid.Tick(1f);
            grid.Tick(2f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(2));
        }

        [Test]
        public void Tick_DifferentSeeds_RegrowDifferentCells()
        {
            var first = CreateCleared(0.5f, 1);
            var second = CreateCleared(0.5f, 2);

            first.Tick(1f);
            second.Tick(1f);

            var differing = 0;
            for (var y = 0; y < first.Height; y++)
            {
                for (var x = 0; x < first.Width; x++)
                {
                    differing += first.GetDepth(x, y) != second.GetDepth(x, y) ? 1 : 0;
                }
            }

            Assert.That(differing, Is.GreaterThan(0));
        }

        private static SnowGrid CreateCleared(float chance, int seed)
        {
            var settings = SnowTestSettings.Create();
            settings.RegrowthChance = chance;
            var grid = new SnowGrid(settings, VehicleArena.Create(), seed);
            grid.Scrape(WholeGrid, int.MaxValue);
            return grid;
        }

        private static int CountAtDepth(SnowGrid grid, int depth)
        {
            var count = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    count += grid.GetDepth(x, y) == depth ? 1 : 0;
                }
            }

            return count;
        }

        private static void AssertSameDepths(SnowGrid expected, SnowGrid actual)
        {
            for (var y = 0; y < expected.Height; y++)
            {
                for (var x = 0; x < expected.Width; x++)
                {
                    Assert.That(actual.GetDepth(x, y), Is.EqualTo(expected.GetDepth(x, y)), $"cell {x},{y}");
                }
            }
        }
    }
}
