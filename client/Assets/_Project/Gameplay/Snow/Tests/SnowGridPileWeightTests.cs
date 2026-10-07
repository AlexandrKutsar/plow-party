using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridPileWeightTests
    {
        private const int Unlimited = int.MaxValue;

        private static readonly Vector2 FirstCellCentre = new Vector2(0.25f, 0.25f);
        private static readonly Vector2 SecondCellCentre = new Vector2(0.75f, 0.25f);
        private static readonly SnowBlade OverFirstCell = new SnowBlade(FirstCellCentre, Vector2.up, 0.2f, 0.2f);
        private static readonly SnowBlade OverFirstTwoCells = new SnowBlade(new Vector2(0.5f, 0.25f), Vector2.up, 0.9f, 0.2f);

        [Test]
        public void Scrape_PileDeeperThanCap_TakesOnlyCapStepsPerCall()
        {
            var grid = Create(2);
            grid.Spill(FirstCellCentre, 10);

            var taken = grid.Scrape(OverFirstCell, Unlimited);

            Assert.That(taken, Is.EqualTo(2));
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(11));
        }

        [Test]
        public void Scrape_CapStopsInsidePile_LeavesSnowUnderPileForLater()
        {
            var grid = Create(2);
            grid.Spill(FirstCellCentre, 3);

            var first = grid.Scrape(OverFirstCell, Unlimited);
            var second = grid.Scrape(OverFirstCell, Unlimited);

            Assert.That(first, Is.EqualTo(2));
            Assert.That(second, Is.EqualTo(4));
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Scrape_TwoPileCells_ShareOneCap()
        {
            var grid = Create(3);
            grid.Spill(FirstCellCentre, 12);
            grid.Spill(SecondCellCentre, 12);

            var taken = grid.Scrape(OverFirstTwoCells, Unlimited);

            Assert.That(taken, Is.EqualTo(3));
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(12));
            Assert.That(grid.GetDepth(1, 0), Is.EqualTo(15));
        }

        [Test]
        public void Scrape_PlainCellBesidePile_IsNotCapped()
        {
            var grid = Create(1);
            grid.Spill(FirstCellCentre, 1);

            var taken = grid.Scrape(OverFirstTwoCells, Unlimited);

            Assert.That(taken, Is.EqualTo(7));
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
            Assert.That(grid.GetDepth(1, 0), Is.EqualTo(0));
        }

        [Test]
        public void SpeedMultiplierUnder_BladeOverPileCell_AppliesPenalty()
        {
            var grid = Create(Unlimited);
            grid.Spill(FirstCellCentre, 1);

            Assert.That(grid.SpeedMultiplierUnder(OverFirstTwoCells), Is.EqualTo(0.6f).Within(1e-5f));
        }

        [Test]
        public void SpeedMultiplierUnder_BladeOverFullSnow_IsOne()
        {
            var grid = Create(Unlimited);

            Assert.That(grid.SpeedMultiplierUnder(OverFirstTwoCells), Is.EqualTo(1f));
        }

        [Test]
        public void SpeedMultiplierUnder_PileOutsideBlade_IsOne()
        {
            var grid = Create(Unlimited);
            grid.Spill(new Vector2(3.75f, 1.75f), 1);

            Assert.That(grid.SpeedMultiplierUnder(OverFirstTwoCells), Is.EqualTo(1f));
        }

        [Test]
        public void SpeedMultiplierUnder_PileScrapedAway_IsOne()
        {
            var grid = Create(Unlimited);
            grid.Spill(FirstCellCentre, 5);
            grid.Scrape(OverFirstCell, Unlimited);

            Assert.That(grid.SpeedMultiplierUnder(OverFirstCell), Is.EqualTo(1f));
        }

        private static SnowGrid Create(int pileStepsPerScrape)
        {
            var settings = SnowTestSettings.Create();
            settings.PileStepsPerScrape = pileStepsPerScrape;
            return new SnowGrid(settings, VehicleArena.Create(), SnowTestSettings.Seed);
        }
    }
}
