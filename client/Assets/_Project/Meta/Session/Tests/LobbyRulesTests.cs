using NUnit.Framework;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Session.Tests
{
    public sealed class LobbyRulesTests
    {
        private const int MaxSlots = 6;

        [Test]
        public void ShouldStart_QuickPlaySearching_IsFalse()
        {
            Assert.That(LobbyRules.ShouldStart(LobbyMode.QuickPlay, 2, MaxSlots, 4.5f, false), Is.False);
        }

        [Test]
        public void ShouldStart_QuickPlayTimerOver_IsTrue()
        {
            Assert.That(LobbyRules.ShouldStart(LobbyMode.QuickPlay, 1, MaxSlots, 0f, false), Is.True);
        }

        [Test]
        public void ShouldStart_QuickPlayFull_IsTrueBeforeTimer()
        {
            Assert.That(LobbyRules.ShouldStart(LobbyMode.QuickPlay, MaxSlots, MaxSlots, 8f, false), Is.True);
        }

        [Test]
        public void ShouldStart_RoomTimerOver_WaitsForHost()
        {
            Assert.That(LobbyRules.ShouldStart(LobbyMode.Room, 3, MaxSlots, 0f, false), Is.False);
        }

        [Test]
        public void ShouldStart_RoomStartRequested_IsTrue()
        {
            Assert.That(LobbyRules.ShouldStart(LobbyMode.Room, 1, MaxSlots, 0f, true), Is.True);
        }

        [Test]
        public void ShouldStart_NoPlayersYet_IsFalse()
        {
            Assert.That(LobbyRules.ShouldStart(LobbyMode.Room, 0, MaxSlots, 0f, true), Is.False);
            Assert.That(LobbyRules.ShouldStart(LobbyMode.QuickPlay, 0, MaxSlots, 0f, false), Is.False);
        }

        [Test]
        public void LineupFor_PlayersFound_ExpectsThemAll()
        {
            var lineup = LobbyRules.LineupFor(3, MaxSlots);

            Assert.That(lineup.ExpectedHumans, Is.EqualTo(3));
            Assert.That(lineup.MaxSlots, Is.EqualTo(MaxSlots));
        }

        [TestCase(0, 1)]
        [TestCase(9, MaxSlots)]
        public void LineupFor_CountOutOfRange_IsClamped(int players, int expected)
        {
            Assert.That(LobbyRules.LineupFor(players, MaxSlots).ExpectedHumans, Is.EqualTo(expected));
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
