using NUnit.Framework;
using PlowParty.Meta.Session.Simulation;
using PlowParty.Shared;

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
            var matchmakingResult = LobbyRules.MatchmakingResultFor(3, MaxSlots, PartyMode.CustomGame);

            Assert.That(matchmakingResult.ExpectedHumans, Is.EqualTo(3));
            Assert.That(matchmakingResult.MaxSlots, Is.EqualTo(MaxSlots));
            Assert.That(matchmakingResult.Mode, Is.EqualTo(PartyMode.CustomGame));
        }

        [TestCase(0, 1)]
        [TestCase(9, MaxSlots)]
        public void MatchmakingResultFor_CountOutOfRange_IsClamped(int players, int expected)
        {
            Assert.That(LobbyRules.MatchmakingResultFor(players, MaxSlots, PartyMode.QuickPlay).ExpectedHumans, Is.EqualTo(expected));
        }

        [Test]
        public void StartsAt_OpenedLobby_StartsAfterTheSearchTime()
        {
            Assert.That(LobbyRules.StartsAt(50_000L, 10f), Is.EqualTo(60_000L));
        }

        [TestCase(10_000L, 0L, 10f)]
        [TestCase(10_000L, 7_500L, 2.5f)]
        [TestCase(10_000L, 12_000L, 0f)]
        public void SecondsLeft_Deadline_CountsDownToZero(long deadline, long now, float expected)
        {
            Assert.That(LobbyRules.SecondsLeft(deadline, now), Is.EqualTo(expected).Within(0.001f));
        }
    }
}
