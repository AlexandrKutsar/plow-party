using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Meta.Tournament.Api;
using PlowParty.Meta.Tournament.Simulation;

namespace PlowParty.Meta.Tournament.Tests
{
    public sealed class TournamentMappingTests
    {
        private const string Me = "me";

        [Test]
        public void ToBoard_MeInTop_MarksMyRowAndAddsNoGap()
        {
            var top = Leaderboard(Row(1, "a", 900), Row(2, Me, 800), Row(3, "c", 700));
            var aroundMe = Leaderboard(Row(1, "a", 900), Row(2, Me, 800), Row(3, "c", 700));
            aroundMe.Me = Row(2, Me, 800);

            var board = TournamentMapping.ToBoard(top, aroundMe, Medals(null), Me);

            Assert.That(board.Rows.Count, Is.EqualTo(3));
            Assert.That(board.Rows[1].IsMe, Is.True);
            Assert.That(board.Rows[0].IsMe, Is.False);
            Assert.That(board.MyRank, Is.EqualTo(2));
        }

        [Test]
        public void ToBoard_MeBelowTop_AddsGapThenMyNeighbours()
        {
            var top = Leaderboard(Row(1, "a", 900), Row(2, "b", 800));
            var aroundMe = Leaderboard(Row(6, "f", 300), Row(7, Me, 200), Row(8, "h", 100));
            aroundMe.Me = Row(7, Me, 200);

            var board = TournamentMapping.ToBoard(top, aroundMe, Medals(null), Me);

            Assert.That(board.Rows.Count, Is.EqualTo(6));
            Assert.That(board.Rows[2].IsGap, Is.True);
            Assert.That(board.Rows[3].Rank, Is.EqualTo(6));
            Assert.That(board.Rows[4].IsMe, Is.True);
        }

        [Test]
        public void ToBoard_NeighboursOverlapTop_SkipsDuplicatesWithoutGap()
        {
            var top = Leaderboard(Row(1, "a", 900), Row(2, "b", 800), Row(3, "c", 700));
            var aroundMe = Leaderboard(Row(2, "b", 800), Row(3, "c", 700), Row(4, Me, 600), Row(5, "e", 500));
            aroundMe.Me = Row(4, Me, 600);

            var board = TournamentMapping.ToBoard(top, aroundMe, Medals(null), Me);

            Assert.That(board.Rows.Count, Is.EqualTo(5));
            Assert.That(board.Rows[3].IsMe, Is.True);
            Assert.That(board.Rows[3].IsGap, Is.False);
        }

        [Test]
        public void ToBoard_NotPlayedToday_HasNoRank()
        {
            var board = TournamentMapping.ToBoard(Leaderboard(Row(1, "a", 900)), Leaderboard(), Medals(null), Me);

            Assert.That(board.MyRank, Is.EqualTo(0));
            Assert.That(board.Rows.Count, Is.EqualTo(1));
        }

        [TestCase("gold", Medal.Gold)]
        [TestCase("silver", Medal.Silver)]
        [TestCase("bronze", Medal.Bronze)]
        [TestCase(null, Medal.None)]
        [TestCase("platinum", Medal.None)]
        public void ToBoard_MedalOfMine_IsMapped(string medal, Medal expected)
        {
            var board = TournamentMapping.ToBoard(Leaderboard(), Leaderboard(), Medals(medal), Me);

            Assert.That(board.Medal, Is.EqualTo(expected));
        }

        [Test]
        public void ToBoard_PlayersCount_ComesFromTop()
        {
            var top = Leaderboard(Row(1, "a", 900));
            top.Players = 42;

            Assert.That(TournamentMapping.ToBoard(top, Leaderboard(), Medals(null), Me).Players, Is.EqualTo(42));
        }

        private static LeaderboardResponse Leaderboard(params StandingResponse[] rows)
        {
            return new LeaderboardResponse { Day = "2026-10-07", Entries = new List<StandingResponse>(rows), Players = rows.Length };
        }

        private static StandingResponse Row(int rank, string accountId, int score)
        {
            return new StandingResponse { Rank = rank, AccountId = accountId, Nickname = accountId.ToUpperInvariant(), Score = score };
        }

        private static MedalsResponse Medals(string medal)
        {
            return new MedalsResponse { Medals = new List<MedalResponse> { new MedalResponse { AccountId = Me, Medal = medal } } };
        }
    }
}
