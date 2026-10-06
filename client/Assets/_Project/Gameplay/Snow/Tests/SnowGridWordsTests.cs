using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowGridWordsTests
    {
        [Test]
        public void WordCount_ThirtyTwoCells_PacksEightCellsPerWord()
        {
            var grid = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);

            Assert.That(grid.WordCount, Is.EqualTo(4));
        }

        [Test]
        public void SetWord_WordsCopiedFromAnotherGrid_ReproducesItsDepths()
        {
            var source = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);
            source.Scrape(new SnowBlade(new Vector2(1f, 1.25f), Vector2.up, 1f, 0.4f), int.MaxValue);
            source.Spill(new Vector2(3.75f, 1.75f), 20);
            var copy = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);

            for (var i = 0; i < source.WordCount; i++)
            {
                copy.SetWord(i, source.GetWord(i));
            }

            for (var y = 0; y < source.Height; y++)
            {
                for (var x = 0; x < source.Width; x++)
                {
                    Assert.That(copy.GetDepth(x, y), Is.EqualTo(source.GetDepth(x, y)), $"cell {x},{y}");
                }
            }
        }

        [Test]
        public void GetWord_FirstWord_HoldsFirstEightCellsFourBitsEach()
        {
            var grid = new SnowGrid(SnowTestSettings.Create(), VehicleArena.Create(), SnowTestSettings.Seed);
            grid.Spill(new Vector2(0.25f, 0.25f), 12);

            Assert.That(grid.GetWord(0), Is.EqualTo(0x3333333F));
        }

        [Test]
        public void Tick_SameSeedAndInputs_ProducesIdenticalWords()
        {
            var first = Play(SnowTestSettings.Seed);
            var second = Play(SnowTestSettings.Seed);

            for (var i = 0; i < first.WordCount; i++)
            {
                Assert.That(second.GetWord(i), Is.EqualTo(first.GetWord(i)));
            }
        }

        private static SnowGrid Play(int seed)
        {
            var settings = SnowTestSettings.Create();
            settings.RegrowthChance = 0.3f;
            settings.BlizzardTimes = new[] { 4f };
            var grid = new SnowGrid(settings, VehicleArena.Create(), seed);
            for (var tick = 1; tick <= 100; tick++)
            {
                var angle = tick * 0.2f;
                grid.Scrape(new SnowBlade(new Vector2(2f + Mathf.Cos(angle), 1f + Mathf.Sin(angle) * 0.5f), new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)), 1f, 0.5f), 5);
                grid.Tick(tick * 0.05f);
            }

            return grid;
        }
    }
}
