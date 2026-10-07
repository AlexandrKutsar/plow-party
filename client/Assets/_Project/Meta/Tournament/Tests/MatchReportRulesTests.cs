using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Meta.Tournament.Simulation;

namespace PlowParty.Meta.Tournament.Tests
{
    public sealed class MatchReportRulesTests
    {
        private const string Host = "3a5392be-0263-4447-9b05-bd21cbe7c786";
        private const string Friend = "6b2b8d1e-1f0e-4c55-9a43-7d3a1b2c4e5f";

        [Test]
        public void BuildRoster_PlayersAndBots_KeepsSlotsOrdered()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(2, null), Seat(0, Host), Seat(1, Friend), Seat(3, null) });

            Assert.That(roster.Roster.Count, Is.EqualTo(4));
            Assert.That(roster.Roster[0].Slot, Is.EqualTo(0));
            Assert.That(roster.Roster[0].AccountId, Is.EqualTo(Host));
            Assert.That(roster.Roster[1].AccountId, Is.EqualTo(Friend));
            Assert.That(roster.Roster[2].AccountId, Is.Null);
        }

        [Test]
        public void BuildRoster_OfflineOrMalformedAccount_SeatsABot()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(0, Host), Seat(1, string.Empty), Seat(2, "not-a-uuid") });

            Assert.That(roster.Roster[1].AccountId, Is.Null);
            Assert.That(roster.Roster[2].AccountId, Is.Null);
        }

        [Test]
        public void BuildRoster_SameAccountTwice_KeepsOnlyTheFirstSeat()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(0, Host), Seat(1, Host) });

            Assert.That(roster.Roster[0].AccountId, Is.EqualTo(Host));
            Assert.That(roster.Roster[1].AccountId, Is.Null);
        }

        [Test]
        public void BuildRoster_DuplicateSlot_KeepsTheFirst()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(0, Host), Seat(0, Friend), Seat(1, null) });

            Assert.That(roster.Roster.Count, Is.EqualTo(2));
            Assert.That(roster.Roster[0].AccountId, Is.EqualTo(Host));
        }

        [Test]
        public void CanRegister_HostSeatedInFourSlots_IsTrue()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(0, Host), Seat(1, null), Seat(2, null), Seat(3, null) });

            Assert.That(MatchReportRules.CanRegister(roster, Host), Is.True);
        }

        [Test]
        public void CanRegister_ThreeSlots_IsFalse()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(0, Host), Seat(1, null), Seat(2, null) });

            Assert.That(MatchReportRules.CanRegister(roster, Host), Is.False);
        }

        [Test]
        public void CanRegister_HostNotSeated_IsFalse()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(0, Friend), Seat(1, null), Seat(2, null), Seat(3, null) });

            Assert.That(MatchReportRules.CanRegister(roster, Host), Is.False);
        }

        [Test]
        public void CanRegister_HostOffline_IsFalse()
        {
            var roster = MatchReportRules.BuildRoster(new[] { Seat(0, null), Seat(1, null), Seat(2, null), Seat(3, null) });

            Assert.That(MatchReportRules.CanRegister(roster, string.Empty), Is.False);
        }

        [Test]
        public void SlotMask_RoundTrip_ReturnsSameSlots()
        {
            var slots = new List<int> { 0, 2, 5 };

            Assert.That(MatchReportRules.SlotsOf(MatchReportRules.SlotMask(slots)), Is.EqualTo(slots));
        }

        [Test]
        public void BuildVote_EverySlotScoredOnce_MissingSlotsScoreZero()
        {
            var scores = new Dictionary<int, int> { [0] = 120, [2] = 340, [4] = 99 };

            var vote = MatchReportRules.BuildVote(new[] { 0, 1, 2, 3 }, scores, null);

            Assert.That(vote.Scores.Count, Is.EqualTo(4));
            Assert.That(vote.Scores[0].Score, Is.EqualTo(120));
            Assert.That(vote.Scores[1].Score, Is.EqualTo(0));
            Assert.That(vote.Scores[2].Score, Is.EqualTo(340));
            Assert.That(vote.Scores[3].Slot, Is.EqualTo(3));
            Assert.That(vote.InterruptedAtSeconds, Is.Null);
        }

        [Test]
        public void BuildVote_NegativeScore_IsClampedToZero()
        {
            var vote = MatchReportRules.BuildVote(new[] { 0 }, new Dictionary<int, int> { [0] = -5 }, null);

            Assert.That(vote.Scores[0].Score, Is.EqualTo(0));
        }

        [Test]
        public void BuildVote_Interrupted_CarriesTheSecond()
        {
            var vote = MatchReportRules.BuildVote(new[] { 0 }, new Dictionary<int, int>(), 95);

            Assert.That(vote.InterruptedAtSeconds, Is.EqualTo(95));
        }

        [TestCase(95.7f, true, 95)]
        [TestCase(1.2f, true, 1)]
        [TestCase(179.9f, true, 179)]
        [TestCase(0.6f, false, 0)]
        [TestCase(180f, false, 0)]
        public void TryGetInterruptedSecond_PlayingElapsed_FloorsWithinContract(float elapsed, bool expected, int second)
        {
            Assert.That(MatchReportRules.TryGetInterruptedSecond(elapsed, out var actual), Is.EqualTo(expected));
            if (expected)
            {
                Assert.That(actual, Is.EqualTo(second));
            }
        }

        private static RosterSeat Seat(int slot, string accountId)
        {
            return new RosterSeat(slot, accountId);
        }
    }
}
