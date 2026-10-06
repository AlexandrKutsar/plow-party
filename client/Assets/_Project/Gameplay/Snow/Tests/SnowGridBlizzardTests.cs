using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridBlizzardTests
    {
        private const float FirstWave = 10f;
        private const float SecondWave = 20f;
        private const float Duration = 2f;

        private static readonly SnowBlade WholeGrid = new SnowBlade(new Vector2(2f, 1f), Vector2.up, 4.1f, 2.1f);

        [Test]
        public void Tick_BeforeWave_LeavesClearedCells()
        {
            var grid = CreateCleared(SnowTestSettings.Seed);

            grid.Tick(FirstWave - 0.01f);

            Assert.That(CountAtDepth(grid, 0), Is.EqualTo(32));
        }

        [Test]
        public void Tick_AfterWaveEnds_EveryClearedCellIsFull()
        {
            var grid = CreateCleared(SnowTestSettings.Seed);

            grid.Tick(FirstWave);
            grid.Tick(FirstWave + Duration);

            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(32));
        }

        [Test]
        public void Tick_WaveMissedEntirely_CompletesInOneTick()
        {
            var grid = CreateCleared(SnowTestSettings.Seed);

            grid.Tick(FirstWave + Duration + 1f);

            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(32));
        }

        [Test]
        public void Tick_HalfwayThroughWave_FillsExactlyOneHalfOfTheGrid()
        {
            var grid = CreateCleared(SnowTestSettings.Seed);

            grid.Tick(FirstWave + Duration * 0.5f);

            Assert.That(FilledHalf(grid), Is.Not.Null);
        }

        [Test]
        public void Tick_CellClearedBehindFront_StaysClearedUntilNextWave()
        {
            var grid = CreateCleared(SnowTestSettings.Seed);
            grid.Tick(FirstWave + Duration * 0.5f);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(FirstWave + Duration);

            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(16));
            Assert.That(CountAtDepth(grid, 0), Is.EqualTo(16));
        }

        [Test]
        public void Tick_SecondWave_RefillsCellsClearedAfterFirst()
        {
            var grid = CreateCleared(SnowTestSettings.Seed);
            grid.Tick(FirstWave + Duration);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(SecondWave + Duration);

            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(32));
        }

        [Test]
        public void Tick_Wave_LeavesSnowPileAndObstacleCells()
        {
            var settings = CreateSettings();
            var arena = VehicleArena.Create().AddBox(new Vector2(3.75f, 1.75f), new Vector2(0.1f, 0.1f));
            var grid = new SnowGrid(settings, arena, SnowTestSettings.Seed);
            grid.Scrape(WholeGrid, int.MaxValue);
            grid.Spill(new Vector2(0.25f, 0.25f), 10);

            grid.Tick(FirstWave + Duration);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(10));
            Assert.That(grid.GetDepth(7, 3), Is.EqualTo(0));
            Assert.That(grid.GetDepth(4, 2), Is.EqualTo(3));
        }

        [Test]
        public void Tick_HalfwayThroughWave_DirectionVariesWithSeed()
        {
            var halves = new HashSet<string>();
            for (var seed = 1; seed <= 20; seed++)
            {
                var grid = CreateCleared(seed);
                grid.Tick(FirstWave + Duration * 0.5f);
                halves.Add(FilledHalf(grid));
            }

            Assert.That(halves.Count, Is.GreaterThan(1));
        }

        private static SnowSettings CreateSettings()
        {
            var settings = SnowTestSettings.Create();
            settings.BlizzardTimes = new[] { FirstWave, SecondWave };
            settings.BlizzardDuration = Duration;
            return settings;
        }

        private static SnowGrid CreateCleared(int seed)
        {
            var grid = new SnowGrid(CreateSettings(), VehicleArena.Create(), seed);
            grid.Scrape(WholeGrid, int.MaxValue);
            return grid;
        }

        private static string FilledHalf(SnowGrid grid)
        {
            if (IsFilledExactly(grid, (x, y) => x < 4))
            {
                return "west";
            }

            if (IsFilledExactly(grid, (x, y) => x >= 4))
            {
                return "east";
            }

            if (IsFilledExactly(grid, (x, y) => y < 2))
            {
                return "south";
            }

            if (IsFilledExactly(grid, (x, y) => y >= 2))
            {
                return "north";
            }

            return null;
        }

        private static bool IsFilledExactly(SnowGrid grid, System.Func<int, int, bool> inHalf)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if ((grid.GetDepth(x, y) == 3) != inHalf(x, y))
                    {
                        return false;
                    }
                }
            }

            return true;
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
    }
}
