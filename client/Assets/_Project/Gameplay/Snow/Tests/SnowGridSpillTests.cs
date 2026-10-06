using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridSpillTests
    {
        private static readonly Vector2 CentreOfCell31 = new Vector2(1.75f, 0.75f);

        private SnowGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);
        }

        [Test]
        public void Spill_FewSteps_StackAboveFullSnowOnCellUnderPoint()
        {
            var landed = _grid.Spill(CentreOfCell31, 10);

            Assert.That(landed, Is.EqualTo(10));
            Assert.That(_grid.GetDepth(3, 1), Is.EqualTo(13));
            Assert.That(_grid.GetDepth(2, 1), Is.EqualTo(3));
        }

        [Test]
        public void Spill_MoreThanOneCellHolds_OverflowsIntoSurroundingRingRowByRow()
        {
            var landed = _grid.Spill(CentreOfCell31, 30);

            Assert.That(landed, Is.EqualTo(30));
            Assert.That(_grid.GetDepth(3, 1), Is.EqualTo(15));
            Assert.That(_grid.GetDepth(2, 0), Is.EqualTo(15));
            Assert.That(_grid.GetDepth(3, 0), Is.EqualTo(9));
            Assert.That(_grid.GetDepth(4, 0), Is.EqualTo(3));
        }

        [Test]
        public void Spill_OnClearedCell_FillsFromEmpty()
        {
            _grid.Scrape(new SnowBlade(CentreOfCell31, Vector2.up, 0.4f, 0.4f), int.MaxValue);

            _grid.Spill(CentreOfCell31, 15);

            Assert.That(_grid.GetDepth(3, 1), Is.EqualTo(15));
            Assert.That(_grid.GetDepth(2, 0), Is.EqualTo(3));
        }

        [Test]
        public void Spill_OntoObstacle_SkipsMaskedCells()
        {
            var arena = VehicleArena.Create().AddBox(CentreOfCell31, new Vector2(0.1f, 0.1f));
            var grid = new SnowGrid(SnowTestSettings.Create(), arena, SnowTestSettings.Seed);

            grid.Spill(CentreOfCell31, 12);

            Assert.That(grid.GetDepth(3, 1), Is.EqualTo(0));
            Assert.That(grid.GetDepth(2, 0), Is.EqualTo(15));
        }

        [Test]
        public void Spill_MoreThanGridHolds_ReportsOnlyStepsThatLanded()
        {
            var landed = _grid.Spill(CentreOfCell31, 1000);

            Assert.That(landed, Is.EqualTo(32 * 12));
            Assert.That(_grid.GetDepth(7, 3), Is.EqualTo(15));
        }

        [Test]
        public void Scrape_OverSnowPile_CollectsPileAndSnowBeneath()
        {
            _grid.Spill(CentreOfCell31, 10);

            var steps = _grid.Scrape(new SnowBlade(CentreOfCell31, Vector2.up, 0.4f, 0.4f), int.MaxValue);

            Assert.That(steps, Is.EqualTo(13));
        }
    }
}
