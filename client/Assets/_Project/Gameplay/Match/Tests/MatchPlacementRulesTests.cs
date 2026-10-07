using NUnit.Framework;
using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Tests
{
    public sealed class MatchPlacementRulesTests
    {
        private readonly MatchPlacement[] _placements = new MatchPlacement[6];

        [Test]
        public void Rank_DistinctScores_OrdersByScoreDescending()
        {
            var count = MatchPlacementRules.Rank(new[] { 0, 1, 2 }, new[] { 10, 30, 20 }, 3, _placements);

            Assert.That(count, Is.EqualTo(3));
            AssertPlacement(0, slot: 1, score: 30, place: 1);
            AssertPlacement(1, slot: 2, score: 20, place: 2);
            AssertPlacement(2, slot: 0, score: 10, place: 3);
        }

        [Test]
        public void Rank_TiedScores_SharePlaceAndSkipNext()
        {
            MatchPlacementRules.Rank(new[] { 0, 1, 2, 3 }, new[] { 50, 30, 30, 10 }, 4, _placements);

            Assert.That(_placements[0].Place, Is.EqualTo(1));
            Assert.That(_placements[1].Place, Is.EqualTo(2));
            Assert.That(_placements[2].Place, Is.EqualTo(2));
            Assert.That(_placements[3].Place, Is.EqualTo(4));
        }

        [Test]
        public void Rank_TiedScores_LowerSlotListedFirst()
        {
            MatchPlacementRules.Rank(new[] { 4, 2 }, new[] { 30, 30 }, 2, _placements);

            Assert.That(_placements[0].Slot, Is.EqualTo(2));
            Assert.That(_placements[1].Slot, Is.EqualTo(4));
        }

        [Test]
        public void Rank_AllZero_EveryoneFirst()
        {
            MatchPlacementRules.Rank(new[] { 0, 1, 2 }, new[] { 0, 0, 0 }, 3, _placements);

            Assert.That(_placements[0].Place, Is.EqualTo(1));
            Assert.That(_placements[1].Place, Is.EqualTo(1));
            Assert.That(_placements[2].Place, Is.EqualTo(1));
        }

        [Test]
        public void Rank_CountBelowArrayLength_IgnoresTrailingEntries()
        {
            var count = MatchPlacementRules.Rank(new[] { 0, 1, 9 }, new[] { 5, 7, 99 }, 2, _placements);

            Assert.That(count, Is.EqualTo(2));
            AssertPlacement(0, slot: 1, score: 7, place: 1);
            AssertPlacement(1, slot: 0, score: 5, place: 2);
        }

        [Test]
        public void Rank_CountAboveOutputLength_KeepsOnlyWhatFits()
        {
            var output = new MatchPlacement[2];

            var count = MatchPlacementRules.Rank(new[] { 0, 1, 2 }, new[] { 1, 3, 2 }, 3, output);

            Assert.That(count, Is.EqualTo(2));
            Assert.That(output[0].Slot, Is.EqualTo(1));
            Assert.That(output[1].Slot, Is.EqualTo(2));
        }

        [Test]
        public void Rank_NoParticipants_ReturnsZero()
        {
            Assert.That(MatchPlacementRules.Rank(new int[0], new int[0], 0, _placements), Is.EqualTo(0));
        }

        private void AssertPlacement(int index, int slot, int score, int place)
        {
            Assert.That(_placements[index].Slot, Is.EqualTo(slot));
            Assert.That(_placements[index].Score, Is.EqualTo(score));
            Assert.That(_placements[index].Place, Is.EqualTo(place));
        }
    }
}
