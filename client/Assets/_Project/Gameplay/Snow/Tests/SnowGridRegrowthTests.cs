using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridRegrowthTests
    {
        private const float Delay = 2f;
        private const float Step = 1f;

        private static readonly SnowBlade WholeGrid = new SnowBlade(new Vector2(2f, 1f), Vector2.up, 4.1f, 2.1f);
        private static readonly SnowBlade FirstColumn = new SnowBlade(new Vector2(0.25f, 1f), Vector2.up, 0.2f, 2.1f);
        private static readonly SnowBlade LastColumn = new SnowBlade(new Vector2(3.75f, 1f), Vector2.up, 0.2f, 2.1f);

        [Test]
        public void Tick_BeforeDelayEnds_LeavesClearedCells()
        {
            var grid = CreateCleared();

            grid.Tick(Delay - 0.01f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_AfterDelay_RaisesClearedCellOneStepPerRegrowthStep()
        {
            var grid = CreateCleared();

            grid.Tick(Delay);
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(1));

            grid.Tick(Delay + Step);
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(2));

            grid.Tick(Delay + Step * 1.5f);
            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(2));
        }

        [Test]
        public void Tick_LongAfterDelay_StopsAtFullDepth()
        {
            var grid = CreateCleared();

            grid.Tick(100f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(3));
        }

        [Test]
        public void Tick_PartlyScrapedCell_RegrowsFromItsDepth()
        {
            var grid = Create(RegrowingSettings());
            grid.Scrape(FirstColumn, 1);

            grid.Tick(Delay);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(3));
        }

        [Test]
        public void Tick_UntouchedFullCell_StaysFull()
        {
            var grid = Create(RegrowingSettings());

            grid.Tick(100f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(3));
        }

        [Test]
        public void Tick_CellsClearedEarlier_RefillFirst()
        {
            var grid = Create(RegrowingSettings());
            grid.Scrape(FirstColumn, int.MaxValue);
            grid.Tick(1f);
            grid.Scrape(LastColumn, int.MaxValue);

            grid.Tick(Delay + Step);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(2));
            Assert.That(grid.GetDepth(7, 0), Is.EqualTo(1));
        }

        [Test]
        public void Tick_CellScrapedAgainWhileRegrowing_RestartsDelay()
        {
            var grid = CreateCleared();
            grid.Tick(Delay + Step);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(Delay + Step + Delay - 0.01f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_NeverTouchesSnowPile()
        {
            var grid = CreateCleared();
            grid.Spill(new Vector2(0.25f, 0.25f), 10);

            grid.Tick(100f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(10));
        }

        [Test]
        public void Tick_PileScrapedBelowFull_RegrowsAfterDelay()
        {
            var grid = Create(RegrowingSettings());
            grid.Spill(new Vector2(0.25f, 0.25f), 4);
            grid.Tick(5f);
            grid.Scrape(new SnowBlade(new Vector2(0.25f, 0.25f), Vector2.up, 0.2f, 0.2f), int.MaxValue);

            grid.Tick(5f + Delay);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(1));
        }

        [Test]
        public void Tick_NeverFillsObstacleCells()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(0.25f, 0.25f), new Vector2(0.1f, 0.1f));
            var grid = new SnowGrid(RegrowingSettings(), arena, SnowTestSettings.Seed);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(100f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_ZeroRegrowthStep_DisablesRegrowth()
        {
            var settings = RegrowingSettings();
            settings.RegrowthStep = 0f;
            var grid = Create(settings);
            grid.Scrape(WholeGrid, int.MaxValue);

            grid.Tick(100f);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Tick_SameTimeReachedInSmallSteps_GivesSameGridAsOneStep()
        {
            var oneStep = Create(RegrowingSettings());
            var smallSteps = Create(RegrowingSettings());
            oneStep.Scrape(FirstColumn, int.MaxValue);
            smallSteps.Scrape(FirstColumn, int.MaxValue);
            oneStep.Tick(0.7f);
            for (var tick = 1; tick <= 7; tick++)
            {
                smallSteps.Tick(tick * 0.1f);
            }

            oneStep.Scrape(LastColumn, int.MaxValue);
            smallSteps.Scrape(LastColumn, int.MaxValue);
            oneStep.Tick(4.3f);
            for (var tick = 8; tick <= 43; tick++)
            {
                smallSteps.Tick(tick * 0.1f);
            }

            AssertSameDepths(oneStep, smallSteps);
        }

        [Test]
        public void Tick_TimeGoingBackwards_AppliesNothingTwice()
        {
            var grid = CreateCleared();
            grid.Tick(Delay + Step);

            grid.Tick(1f);
            grid.Tick(Delay + Step);

            Assert.That(grid.GetDepth(0, 0), Is.EqualTo(2));
        }

        private static SnowGrid Create(SnowSettings settings)
        {
            return new SnowGrid(settings, VehicleArena.Create(), SnowTestSettings.Seed);
        }

        private static SnowSettings RegrowingSettings()
        {
            var settings = SnowTestSettings.Create();
            settings.RegrowthDelay = Delay;
            settings.RegrowthStep = Step;
            return settings;
        }

        private static SnowGrid CreateCleared()
        {
            var grid = Create(RegrowingSettings());
            grid.Scrape(WholeGrid, int.MaxValue);
            return grid;
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
