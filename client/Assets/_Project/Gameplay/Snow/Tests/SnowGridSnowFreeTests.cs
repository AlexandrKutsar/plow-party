using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridSnowFreeTests
    {
        private const int FullDepth = 3;
        private const float Wave = 10f;
        private const float Duration = 2f;

        private static readonly ISnowFreeArea FirstCell = new CircleSnowFreeArea(new Vector2(0.25f, 0.25f), 0.1f);
        private static readonly SnowBlade WholeGrid = new SnowBlade(new Vector2(2f, 1f), Vector2.up, 4.1f, 2.1f);

        [Test]
        public void Constructor_SnowFreeCell_StartsCleared()
        {
            var grid = Create(SnowTestSettings.Create(), FirstCell, SnowTestSettings.Seed);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Constructor_CellOutsideSnowFreeArea_StartsFull()
        {
            var grid = Create(SnowTestSettings.Create(), FirstCell, SnowTestSettings.Seed);

            Assert.That(grid.GetDepth(1, 0), Is.EqualTo(FullDepth));
        }

        [Test]
        public void Tick_LongAfterScrape_NeverRaisesSnowFreeCell()
        {
            var settings = SnowTestSettings.Create();
            settings.RegrowthStepInterval = 1f;
            var grid = Create(settings, FirstCell, SnowTestSettings.Seed);
            grid.Spill(new Vector2(0.25f, 0.25f), 2);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(100f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_AfterBlizzard_SnowFreeCellStaysCleared()
        {
            var settings = SnowTestSettings.Create();
            settings.BlizzardTimes = new[] { Wave };
            var grid = Create(settings, FirstCell, SnowTestSettings.Seed);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(Wave + Duration);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
            Assert.That(grid.GetDepth(1, 0), Is.EqualTo(FullDepth));
        }

        [Test]
        public void Spill_OnSnowFreeCell_LeavesPileThere()
        {
            var grid = Create(SnowTestSettings.Create(), FirstCell, SnowTestSettings.Seed);

            var placed = grid.Spill(new Vector2(0.25f, 0.25f), 5);

            Assert.That(placed, Is.EqualTo(5));
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(5));
        }

        [Test]
        public void Scrape_PileOnSnowFreeCell_CollectsIt()
        {
            var grid = Create(SnowTestSettings.Create(), FirstCell, SnowTestSettings.Seed);
            grid.Spill(new Vector2(0.25f, 0.25f), 5);

            var taken = grid.Scrape(new SnowBlade(new Vector2(0.25f, 0.25f), Vector2.up, 0.2f, 0.2f), int.MaxValue);

            Assert.That(taken, Is.EqualTo(5));
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_BlizzardPiles_AvoidSnowFreeCells()
        {
            var settings = SnowTestSettings.Create();
            settings.BlizzardTimes = new[] { Wave };
            settings.BlizzardPilesPerWave = 3;
            settings.BlizzardPileSteps = 1;
            var allButLastCell = new CircleSnowFreeArea(Vector2.zero, 4f);

            for (var seed = 1; seed <= 10; seed++)
            {
                var grid = Create(settings, allButLastCell, seed);

                grid.Tick(Wave + Duration);

                Assert.That(grid.GetDepth(7, 3), Is.EqualTo(FullDepth + 3), $"seed {seed}");
            }
        }

        private static SnowGrid Create(SnowSettings settings, ISnowFreeArea snowFree, int seed)
        {
            return new SnowGrid(settings, VehicleArena.Create(), snowFree, seed);
        }
    }
}
