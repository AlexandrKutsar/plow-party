using NUnit.Framework;
using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Tests
{
    public sealed class MatchRulesPhaseTests
    {
        private MatchRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = MatchTestSettings.CreateRules();
        }

        [Test]
        public void NextPhase_WaitingWithEmptySlots_KeepsWaiting()
        {
            Assert.That(_rules.NextPhase(MatchPhase.WaitingForPlayers, 100f, false), Is.EqualTo(MatchPhase.WaitingForPlayers));
        }

        [Test]
        public void NextPhase_WaitingWithAllSlotsFilled_StartsCountdown()
        {
            Assert.That(_rules.NextPhase(MatchPhase.WaitingForPlayers, 0.5f, true), Is.EqualTo(MatchPhase.Countdown));
        }

        [Test]
        public void NextPhase_EmptySlotsOutsideWaiting_AreIgnored()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Countdown, 3f, false), Is.EqualTo(MatchPhase.Playing));
            Assert.That(_rules.NextPhase(MatchPhase.Playing, 180f, false), Is.EqualTo(MatchPhase.Results));
        }

        [Test]
        public void NextPhase_CountdownBeforeDuration_StaysCountdown()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Countdown, 2.9f, true), Is.EqualTo(MatchPhase.Countdown));
        }

        [Test]
        public void NextPhase_CountdownAtDuration_StartsPlaying()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Countdown, 3f, true), Is.EqualTo(MatchPhase.Playing));
        }

        [Test]
        public void NextPhase_PlayingBeforeDuration_StaysPlaying()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Playing, 179.9f, true), Is.EqualTo(MatchPhase.Playing));
        }

        [Test]
        public void NextPhase_PlayingAtDuration_ShowsResults()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Playing, 180f, true), Is.EqualTo(MatchPhase.Results));
        }

        [Test]
        public void NextPhase_ResultsHoweverLong_StaysResults()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Results, 0f, true), Is.EqualTo(MatchPhase.Results));
            Assert.That(_rules.NextPhase(MatchPhase.Results, 10000f, true), Is.EqualTo(MatchPhase.Results));
        }

        [Test]
        public void IsInputLocked_EachPhase_UnlockedOnlyWhilePlaying()
        {
            Assert.That(MatchRules.IsInputLocked(MatchPhase.WaitingForPlayers), Is.True);
            Assert.That(MatchRules.IsInputLocked(MatchPhase.Countdown), Is.True);
            Assert.That(MatchRules.IsInputLocked(MatchPhase.Playing), Is.False);
            Assert.That(MatchRules.IsInputLocked(MatchPhase.Results), Is.True);
        }
    }
}
