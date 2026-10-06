using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridBlizzardPileTests
    {
        private const float FirstWave = 10f;
        private const float SecondWave = 20f;
        private const float Duration = 2f;
        private const int FullDepth = 3;

        private static readonly SnowBlade WholeGrid = new SnowBlade(new Vector2(2f, 1f), Vector2.up, 4.1f, 2.1f);

        [Test]
        public void Tick_AfterWaveEnds_LeavesEveryPileStepAboveFullSnow()
        {
            var grid = CreateCleared(SnowTestSettings.Seed, 2, 20, VehicleArena.Create());

            grid.Tick(FirstWave + Duration);

            Assert.That(PileSteps(grid), Is.EqualTo(40));
        }

        [Test]
        public void Tick_BeforeWave_LeavesNoPiles()
        {
            var grid = CreateCleared(SnowTestSettings.Seed, 2, 20, VehicleArena.Create());

            grid.Tick(FirstWave - 0.01f);

            Assert.That(PileSteps(grid), Is.EqualTo(0));
        }

        [Test]
        public void Tick_HalfwayThroughWave_DropsPilesOnlyWhereFrontHasPassed()
        {
            var seedsWithPiles = 0;
            for (var seed = 1; seed <= 20; seed++)
            {
                var grid = CreateCleared(seed, 2, 12, VehicleArena.Create());

                grid.Tick(FirstWave + Duration * 0.5f);

                Assert.That(CountAtDepthOrAbove(grid, FullDepth), Is.EqualTo(16), $"seed {seed}");
                seedsWithPiles += PileSteps(grid) > 0 ? 1 : 0;
            }

            Assert.That(seedsWithPiles, Is.GreaterThan(0));
        }

        [Test]
        public void Tick_EachWave_LeavesItsOwnPiles()
        {
            var grid = CreateCleared(SnowTestSettings.Seed, 2, 20, VehicleArena.Create());
            grid.Tick(FirstWave + Duration);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(SecondWave + Duration);

            Assert.That(PileSteps(grid), Is.EqualTo(40));
        }

        [Test]
        public void Tick_DifferentSeeds_DropPilesInDifferentCells()
        {
            var layouts = new HashSet<string>();
            for (var seed = 1; seed <= 10; seed++)
            {
                var grid = CreateCleared(seed, 1, 12, VehicleArena.Create());
                grid.Tick(FirstWave + Duration);
                layouts.Add(PileLayout(grid));
            }

            Assert.That(layouts.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Tick_MostOfGridUnderObstacle_PilesLandOnlyOnOpenCells()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(1.75f, 1f), new Vector2(1.75f, 1f));
            var grid = CreateCleared(SnowTestSettings.Seed, 2, 12, arena);

            grid.Tick(FirstWave + Duration);

            Assert.That(PileSteps(grid), Is.EqualTo(24));
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < 7; x++)
                {
                    Assert.That(grid.GetDepth(x, y), Is.EqualTo(0), $"cell {x},{y}");
                }
            }
        }

        private static SnowGrid CreateCleared(int seed, int pilesPerWave, int pileSteps, VehicleArena arena)
        {
            var settings = SnowTestSettings.Create();
            settings.BlizzardTimes = new[] { FirstWave, SecondWave };
            settings.BlizzardDuration = Duration;
            settings.BlizzardPilesPerWave = pilesPerWave;
            settings.BlizzardPileSteps = pileSteps;
            var grid = new SnowGrid(settings, arena, seed);
            grid.Scrape(WholeGrid, int.MaxValue);
            return grid;
        }

        private static int PileSteps(SnowGrid grid)
        {
            var steps = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    steps += Mathf.Max(0, grid.GetDepth(x, y) - FullDepth);
                }
            }

            return steps;
        }

        private static int CountAtDepthOrAbove(SnowGrid grid, int depth)
        {
            var count = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    count += grid.GetDepth(x, y) >= depth ? 1 : 0;
                }
            }

            return count;
        }

        private static string PileLayout(SnowGrid grid)
        {
            var layout = new System.Text.StringBuilder();
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    layout.Append(grid.GetDepth(x, y) > FullDepth ? '#' : '.');
                }
            }

            return layout.ToString();
        }
    }
}
