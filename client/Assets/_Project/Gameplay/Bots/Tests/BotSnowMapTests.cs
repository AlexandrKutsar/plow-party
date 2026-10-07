using System;
using NUnit.Framework;
using PlowParty.Gameplay.Bots.Simulation;
using UnityEngine;
using Random = System.Random;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class BotSnowMapTests
    {
        private static readonly Vector2[] NoOthers = Array.Empty<Vector2>();

        [Test]
        public void Refresh_FullCover_SumsPlainStepsPerNavCell()
        {
            var map = new BotSnowMap(BotTestSettings.OpenGrid());

            map.Refresh(new FakeSnowDepths(3));

            Assert.That(map.SnowAt(new Vector2Int(5, 5)), Is.EqualTo(12));
            Assert.That(map.PileAt(new Vector2Int(5, 5)), Is.EqualTo(0));
            Assert.That(map.RichnessShare(new Vector2Int(5, 5)), Is.EqualTo(1f));
        }

        [Test]
        public void Refresh_SnowPile_CountsStepsAboveFullAsPile()
        {
            var map = new BotSnowMap(BotTestSettings.OpenGrid());

            map.Refresh(new FakeSnowDepths(0).Fill(new Vector2(0f, 0f), new Vector2(1f, 1f), 15));

            var cell = new Vector2Int(10, 10);
            Assert.That(map.SnowAt(cell), Is.EqualTo(12));
            Assert.That(map.PileAt(cell), Is.EqualTo(48));
        }

        [Test]
        public void TryFindSnow_TwoPatches_PrefersTheNearerOne()
        {
            var map = new BotSnowMap(BotTestSettings.OpenGrid());
            map.Refresh(new FakeSnowDepths(0)
                .Fill(new Vector2(2f, -2f), new Vector2(5f, 2f), 3)
                .Fill(new Vector2(-9f, -9f), new Vector2(-6f, -5f), 3));

            var found = map.TryFindSnow(new Vector2(0f, 0f), Vector2.right, NoOthers, 0, BotTestSettings.Create(), 0f, new Random(1), out var target, out var richness);

            Assert.That(found, Is.True);
            Assert.That(target.x, Is.InRange(1.5f, 5.5f));
            Assert.That(richness, Is.GreaterThan(0.5f));
        }

        [Test]
        public void TryFindSnow_RivalOnThePatch_PicksTheFreeOne()
        {
            var map = new BotSnowMap(BotTestSettings.OpenGrid());
            map.Refresh(new FakeSnowDepths(0)
                .Fill(new Vector2(3f, -2f), new Vector2(6f, 2f), 3)
                .Fill(new Vector2(-6f, -2f), new Vector2(-3f, 2f), 3));
            var others = new[] { new Vector2(4.5f, 0f) };

            map.TryFindSnow(Vector2.zero, Vector2.up, others, 1, BotTestSettings.Create(), 0f, new Random(1), out var target, out _);

            Assert.That(target.x, Is.LessThan(0f));
        }

        [Test]
        public void TryFindSnow_NoSnowAnywhere_ReturnsFalse()
        {
            var map = new BotSnowMap(BotTestSettings.OpenGrid());
            map.Refresh(new FakeSnowDepths(0));

            Assert.That(map.TryFindSnow(Vector2.zero, Vector2.up, NoOthers, 0, BotTestSettings.Create(), 0f, new Random(1), out _, out _), Is.False);
        }

        [Test]
        public void TryFindPile_BigAndSmallPile_SkipsTheOneBelowMinimum()
        {
            var map = new BotSnowMap(BotTestSettings.OpenGrid());
            map.Refresh(new FakeSnowDepths(3)
                .Fill(new Vector2(1f, 1f), new Vector2(1.5f, 1.5f), 4)
                .Fill(new Vector2(-8f, -8f), new Vector2(-7f, -7f), 15));

            var found = map.TryFindPile(Vector2.zero, BotTestSettings.Create(), out var point, out var steps);

            Assert.That(found, Is.True);
            Assert.That(point.x, Is.LessThan(-6f));
            Assert.That(steps, Is.GreaterThanOrEqualTo(20));
        }
    }
}
