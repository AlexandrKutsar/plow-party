using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Meta.Tournament.Simulation;

namespace PlowParty.Meta.Tournament.Tests
{
    public sealed class TournamentTextTests
    {
        [Test]
        public void Summary_Offline_SaysNoConnection()
        {
            Assert.That(TournamentText.Summary(null), Is.EqualTo("Нет связи"));
        }

        [Test]
        public void Summary_NotPlayedToday_InvitesToPlay()
        {
            Assert.That(TournamentText.Summary(Board(0, 12)), Is.EqualTo("Сыграйте матч, чтобы попасть в таблицу"));
        }

        [Test]
        public void Summary_Ranked_ShowsRankOfPlayers()
        {
            Assert.That(TournamentText.Summary(Board(5, 12)), Is.EqualTo("Ваше место: 5 из 12"));
        }

        [TestCase(Medal.Gold, "Медаль: золото")]
        [TestCase(Medal.Silver, "Медаль: серебро")]
        [TestCase(Medal.Bronze, "Медаль: бронза")]
        [TestCase(Medal.None, "")]
        public void MedalLabel_Medal_NamesIt(Medal medal, string expected)
        {
            Assert.That(TournamentText.MedalLabel(medal), Is.EqualTo(expected));
        }

        [Test]
        public void Rank_Standing_ShowsRankWithDot()
        {
            Assert.That(TournamentText.Rank(new Standing(3, "Fox", 1200, false)), Is.EqualTo("3."));
        }

        [Test]
        public void Rank_Gap_ShowsEllipsis()
        {
            Assert.That(TournamentText.Rank(Standing.Gap), Is.EqualTo("…"));
        }

        private static TournamentBoard Board(int myRank, int players)
        {
            return new TournamentBoard(new List<Standing>(), myRank, players, Medal.None);
        }
    }
}
