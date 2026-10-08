using NUnit.Framework;
using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Tests
{
    public sealed class MatchRulesClockTests
    {
        private MatchRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = MatchTestSettings.CreateRules();
        }

        [Test]
        public void PhaseRemaining_PartwayThroughPhase_IsDurationMinusElapsed()
        {
            Assert.That(_rules.PhaseRemaining(MatchPhase.Countdown, 1f), Is.EqualTo(2f));
            Assert.That(_rules.PhaseRemaining(MatchPhase.Playing, 30f), Is.EqualTo(150f));
        }

        [Test]
        public void PhaseRemaining_DuringResults_IsZeroBecauseResultsHasNoTimer()
        {
            Assert.That(_rules.PhaseRemaining(MatchPhase.Results, 5f), Is.EqualTo(0f));
        }

        [Test]
        public void PhaseRemaining_PastDuration_IsZero()
        {
            Assert.That(_rules.PhaseRemaining(MatchPhase.Playing, 200f), Is.EqualTo(0f));
        }

        [Test]
        public void PhaseRemaining_WhileWaiting_CountsDownTheWaitingCap()
        {
            Assert.That(_rules.PhaseRemaining(MatchPhase.WaitingForPlayers, 4f), Is.EqualTo(11f));
        }

        [Test]
        public void PlayingElapsed_WhileWaiting_IsZeroAndAllPlayingRemains()
        {
            Assert.That(_rules.PlayingElapsed(MatchPhase.WaitingForPlayers, 9f), Is.EqualTo(0f));
            Assert.That(_rules.PlayingRemaining(MatchPhase.WaitingForPlayers, 9f), Is.EqualTo(180f));
        }

        [Test]
        public void PlayingElapsed_DuringCountdown_IsZero()
        {
            Assert.That(_rules.PlayingElapsed(MatchPhase.Countdown, 2f), Is.EqualTo(0f));
        }

        [Test]
        public void PlayingElapsed_WhilePlaying_IsPhaseElapsedCappedAtDuration()
        {
            Assert.That(_rules.PlayingElapsed(MatchPhase.Playing, 45.5f), Is.EqualTo(45.5f));
            Assert.That(_rules.PlayingElapsed(MatchPhase.Playing, 181f), Is.EqualTo(180f));
        }

        [Test]
        public void PlayingElapsed_DuringResults_IsFullDuration()
        {
            Assert.That(_rules.PlayingElapsed(MatchPhase.Results, 1f), Is.EqualTo(180f));
        }

        [Test]
        public void PlayingRemaining_EachPhase_CountsDownOnlyWhilePlaying()
        {
            Assert.That(_rules.PlayingRemaining(MatchPhase.Countdown, 2f), Is.EqualTo(180f));
            Assert.That(_rules.PlayingRemaining(MatchPhase.Playing, 60f), Is.EqualTo(120f));
            Assert.That(_rules.PlayingRemaining(MatchPhase.Results, 2f), Is.EqualTo(0f));
        }

        [Test]
        public void PhaseElapsed_FromTicks_IsTickCountTimesDeltaTime()
        {
            Assert.That(MatchRules.PhaseElapsed(160, 100, 1f / 60f), Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void PhaseElapsed_TickBeforePhaseStart_IsZero()
        {
            Assert.That(MatchRules.PhaseElapsed(90, 100, 1f / 60f), Is.EqualTo(0f));
        }
    }
}
