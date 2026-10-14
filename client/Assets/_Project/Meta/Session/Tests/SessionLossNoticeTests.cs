using NUnit.Framework;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Session.Tests
{
    public sealed class SessionLossNoticeTests
    {
        [Test]
        public void For_HostLeftWhilePlaying_SaysTheHostLeftAndTheMatchIsInterrupted()
        {
            Assert.That(SessionLossNotice.For(MatchStanding.Playing), Is.EqualTo(SessionNotice.HostLeft));
        }

        [Test]
        public void For_HostLeftBeforePlaying_SaysTheConnectionIsLost()
        {
            Assert.That(SessionLossNotice.For(MatchStanding.BeforePlaying), Is.EqualTo(SessionNotice.ConnectionLost));
        }

        [Test]
        public void For_TurnedAwayByTheHost_SaysTheMatchAlreadyStarted()
        {
            Assert.That(SessionLossNotice.For(MatchStanding.TurnedAway), Is.EqualTo(SessionNotice.MatchStarted));
        }

        [Test]
        public void KeepsResults_HostLeftOnResults_StaysOnTheFrozenResults()
        {
            Assert.That(SessionLossNotice.KeepsResults(MatchStanding.Finished), Is.True);
        }

        [TestCase(MatchStanding.BeforePlaying)]
        [TestCase(MatchStanding.Playing)]
        [TestCase(MatchStanding.TurnedAway)]
        public void KeepsResults_LostBeforeResults_ReturnsToTheMenu(MatchStanding standing)
        {
            Assert.That(SessionLossNotice.KeepsResults(standing), Is.False);
        }
    }
}
