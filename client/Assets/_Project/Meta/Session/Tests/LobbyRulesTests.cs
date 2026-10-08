using NUnit.Framework;
using PlowParty.Infrastructure.Network;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Session.Tests
{
    public sealed class LobbyRulesTests
    {
        private const int MaxSlots = 6;

        [Test]
        public void ShouldStart_Searching_IsFalse()
        {
            Assert.That(LobbyRules.ShouldStart(2, MaxSlots, 4.5f), Is.False);
        }

        [Test]
        public void ShouldStart_TimerOver_IsTrue()
        {
            Assert.That(LobbyRules.ShouldStart(1, MaxSlots, 0f), Is.True);
        }

        [Test]
        public void ShouldStart_Full_IsTrueBeforeTimer()
        {
            Assert.That(LobbyRules.ShouldStart(MaxSlots, MaxSlots, 8f), Is.True);
        }

        [Test]
        public void ShouldStart_NoPlayersYet_IsFalse()
        {
            Assert.That(LobbyRules.ShouldStart(0, MaxSlots, 0f), Is.False);
        }

        [Test]
        public void MatchmakingResultFor_PlayersFound_ExpectsThemAll()
        {
            var matchmakingResult = LobbyRules.MatchmakingResultFor(3, MaxSlots);

            Assert.That(matchmakingResult.ExpectedHumans, Is.EqualTo(3));
            Assert.That(matchmakingResult.MaxSlots, Is.EqualTo(MaxSlots));
        }

        [TestCase(0, 1)]
        [TestCase(9, MaxSlots)]
        public void MatchmakingResultFor_CountOutOfRange_IsClamped(int players, int expected)
        {
            Assert.That(LobbyRules.MatchmakingResultFor(players, MaxSlots).ExpectedHumans, Is.EqualTo(expected));
        }

        [TestCase(10_000L, 0L, 10f)]
        [TestCase(10_000L, 7_500L, 2.5f)]
        [TestCase(10_000L, 12_000L, 0f)]
        public void SecondsLeft_Deadline_CountsDownToZero(long deadline, long now, float expected)
        {
            Assert.That(LobbyRules.SecondsLeft(deadline, now), Is.EqualTo(expected).Within(0.001f));
        }

        [TestCase(SessionStartOutcome.NotFound)]
        [TestCase(SessionStartOutcome.Refused)]
        public void FoundNothingToJoin_NoOpenSession_IsTrue(SessionStartOutcome outcome)
        {
            Assert.That(LobbyRules.FoundNothingToJoin(outcome), Is.True);
        }

        [TestCase(SessionStartOutcome.Started)]
        [TestCase(SessionStartOutcome.Failed)]
        public void FoundNothingToJoin_JoinedOrBroken_IsFalse(SessionStartOutcome outcome)
        {
            Assert.That(LobbyRules.FoundNothingToJoin(outcome), Is.False);
        }

        [TestCase(0d, 0.3f)]
        [TestCase(0.5d, 0.9f)]
        [TestCase(1d, 1.5f)]
        public void HostJitterSeconds_Roll_SpansTheWindow(double roll, float expected)
        {
            Assert.That(LobbyRules.HostJitterSeconds(roll, 0.3f, 1.5f), Is.EqualTo(expected).Within(1e-5f));
        }

        [Test]
        public void HostJitterSeconds_RollOutOfRange_StaysInsideTheWindow()
        {
            Assert.That(LobbyRules.HostJitterSeconds(7d, 0.3f, 1.5f), Is.EqualTo(1.5f).Within(1e-5f));
            Assert.That(LobbyRules.HostJitterSeconds(-1d, 0.3f, 1.5f), Is.EqualTo(0.3f).Within(1e-5f));
        }

        [Test]
        public void HostJitterSeconds_InvertedWindow_IsTheMinimum()
        {
            Assert.That(LobbyRules.HostJitterSeconds(0.8d, 1f, 0.5f), Is.EqualTo(1f));
        }
    }
}
