using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridScrapeTests
    {
        private const int Unlimited = int.MaxValue;

        private SnowGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);
        }

        [Test]
        public void Scrape_BladeFacingNorth_ClearsCellsWithCentreUnderBladeAndReturnsSteps()
        {
            var steps = _grid.Scrape(new SnowBlade(new Vector2(1f, 1.25f), Vector2.up, 1f, 0.4f), Unlimited);

            Assert.That(steps, Is.EqualTo(6));
            Assert.That(_grid.GetDepth(1, 2), Is.EqualTo(0));
            Assert.That(_grid.GetDepth(2, 2), Is.EqualTo(0));
            Assert.That(_grid.GetDepth(0, 2), Is.EqualTo(3));
            Assert.That(_grid.GetDepth(3, 2), Is.EqualTo(3));
            Assert.That(_grid.GetDepth(1, 1), Is.EqualTo(3));
            Assert.That(_grid.GetDepth(1, 3), Is.EqualTo(3));
        }

        [Test]
        public void Scrape_BladeFacingEast_WidthRunsAcrossFacing()
        {
            var steps = _grid.Scrape(new SnowBlade(new Vector2(1.25f, 1f), Vector2.right, 1f, 0.4f), Unlimited);

            Assert.That(steps, Is.EqualTo(6));
            Assert.That(_grid.GetDepth(2, 1), Is.EqualTo(0));
            Assert.That(_grid.GetDepth(2, 2), Is.EqualTo(0));
            Assert.That(_grid.GetDepth(1, 1), Is.EqualTo(3));
            Assert.That(_grid.GetDepth(3, 1), Is.EqualTo(3));
        }

        [Test]
        public void Scrape_BladeAtDiagonal_ClearsOnlyCellsInsideRotatedStrip()
        {
            var forward = new Vector2(1f, 1f).normalized;

            var steps = _grid.Scrape(new SnowBlade(new Vector2(1.5f, 1f), forward, 1.5f, 0.3f), Unlimited);

            Assert.That(steps, Is.EqualTo(6));
            Assert.That(_grid.GetDepth(2, 2), Is.EqualTo(0));
            Assert.That(_grid.GetDepth(3, 1), Is.EqualTo(0));
            Assert.That(_grid.GetDepth(2, 1), Is.EqualTo(3));
            Assert.That(_grid.GetDepth(3, 2), Is.EqualTo(3));
            Assert.That(_grid.GetDepth(1, 3), Is.EqualTo(3));
            Assert.That(_grid.GetDepth(4, 0), Is.EqualTo(3));
        }

        [Test]
        public void Scrape_RoomSmallerThanSnow_StopsAfterRoomInRowMajorOrder()
        {
            var steps = _grid.Scrape(new SnowBlade(new Vector2(1f, 1.25f), Vector2.up, 1f, 0.4f), 4);

            Assert.That(steps, Is.EqualTo(4));
            Assert.That(_grid.GetDepth(1, 2), Is.EqualTo(0));
            Assert.That(_grid.GetDepth(2, 2), Is.EqualTo(2));
        }

        [Test]
        public void Scrape_NoRoom_LeavesSnow()
        {
            var steps = _grid.Scrape(new SnowBlade(new Vector2(1f, 1.25f), Vector2.up, 1f, 0.4f), 0);

            Assert.That(steps, Is.EqualTo(0));
            Assert.That(_grid.GetDepth(1, 2), Is.EqualTo(3));
        }

        [Test]
        public void Scrape_SameCellsTwice_SecondPassFindsNothing()
        {
            var blade = new SnowBlade(new Vector2(1f, 1.25f), Vector2.up, 1f, 0.4f);
            _grid.Scrape(blade, Unlimited);

            Assert.That(_grid.Scrape(blade, Unlimited), Is.EqualTo(0));
        }

        [Test]
        public void Scrape_BladeOverObstacle_TakesOnlyUnmaskedCells()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(0.75f, 1.25f), new Vector2(0.1f, 0.1f));
            var grid = new SnowGrid(SnowTestSettings.Create(), arena, SnowTestSettings.Seed);

            var steps = grid.Scrape(new SnowBlade(new Vector2(1f, 1.25f), Vector2.up, 1f, 0.4f), Unlimited);

            Assert.That(steps, Is.EqualTo(3));
            Assert.That(grid.GetDepth(1, 2), Is.EqualTo(0));
            Assert.That(grid.GetDepth(2, 2), Is.EqualTo(0));
        }

        [Test]
        public void Scrape_BladePartlyOutsideGrid_ClearsOnlyCellsInside()
        {
            var steps = _grid.Scrape(new SnowBlade(new Vector2(0f, 0.25f), Vector2.up, 1f, 0.4f), Unlimited);

            Assert.That(steps, Is.EqualTo(3));
            Assert.That(_grid.GetDepth(0, 0), Is.EqualTo(0));
        }
    }
}
