using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridResetTests
    {
        private const float Delay = 2f;
        private const float Step = 1f;
        private const float Wave = 10f;
        private const int CellCount = 32;

        private static readonly SnowBlade WholeGrid = new SnowBlade(new Vector2(2f, 1f), Vector2.up, 4.1f, 2.1f);
        private static readonly SnowBlade FirstColumn = new SnowBlade(new Vector2(0.25f, 1f), Vector2.up, 0.2f, 2.1f);

        [Test]
        public void Reset_AfterScrape_RefillsEveryCell()
        {
            var grid = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Reset(SnowTestSettings.Seed);

            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(CellCount));
        }

        [Test]
        public void Reset_AfterSpill_RemovesSnowPiles()
        {
            var grid = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);
            grid.Spill(new Vector2(1f, 1f), 20);

            grid.Reset(SnowTestSettings.Seed);

            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(CellCount));
        }

        [Test]
        public void Reset_MaskedAndSnowFreeCells_StayEmpty()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(3.75f, 1.75f), new Vector2(0.1f, 0.1f));
            var snowFree = new CircleSnowFreeArea(new Vector2(0.25f, 0.25f), 0.1f);
            var grid = new SnowGrid(SnowTestSettings.Create(), arena, snowFree, SnowTestSettings.Seed);
            grid.Spill(new Vector2(0.25f, 0.25f), 5);

            grid.Reset(SnowTestSettings.Seed);

            Assert.That(grid.GetDepth(7, 3), Is.EqualTo(0));
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(CellCount - 2));
        }

        [Test]
        public void Reset_ClockRestartsAtZero_RegrowthRunsOnNewMatchTime()
        {
            var grid = new SnowGrid(RegrowingSettings(), VehicleArena.Create(), SnowTestSettings.Seed);
            grid.Tick(100f);

            grid.Reset(SnowTestSettings.Seed);
            grid.Scrape(FirstColumn, int.MaxValue);
            grid.Tick(Delay);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(1));
        }

        [Test]
        public void Reset_BlizzardAlreadyPassed_RunsAgainInNextMatch()
        {
            var settings = SnowTestSettings.Create();
            settings.BlizzardTimes = new[] { Wave };
            settings.BlizzardDuration = 0f;
            var grid = new SnowGrid(settings, VehicleArena.Create(), SnowTestSettings.Seed);
            grid.Tick(Wave);

            grid.Reset(SnowTestSettings.Seed);
            grid.Scrape(WholeGrid, int.MaxValue);
            grid.Tick(Wave);

            Assert.That(CountAtDepth(grid, 3), Is.EqualTo(CellCount));
        }

        private static SnowSettings RegrowingSettings()
        {
            var settings = SnowTestSettings.Create();
            settings.RegrowthDelay = Delay;
            settings.RegrowthStepInterval = Step;
            return settings;
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
