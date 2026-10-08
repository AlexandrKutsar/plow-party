using NUnit.Framework;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Session.Tests
{
    public sealed class SessionLossNoticeTests
    {
        [Test]
        public void For_MatchUnderway_SaysTheHostLeftAndTheMatchIsInterrupted()
        {
            Assert.That(SessionLossNotice.For(MatchStanding.Underway), Is.EqualTo("Хост вышел, матч прерван"));
        }

        [Test]
        public void For_MatchFinished_ReturnsQuietly()
        {
            Assert.That(SessionLossNotice.For(MatchStanding.Finished), Is.Null);
        }

        [Test]
        public void For_MatchNeverSeenRunning_SaysTheConnectionIsLost()
        {
            Assert.That(SessionLossNotice.For(MatchStanding.NotStarted), Is.EqualTo("Связь с хостом потеряна"));
        }
    }
}
