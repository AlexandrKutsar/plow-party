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
        public void NextPhase_CountdownBeforeDuration_StaysCountdown()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Countdown, 2.9f, false), Is.EqualTo(MatchPhase.Countdown));
        }

        [Test]
        public void NextPhase_CountdownAtDuration_StartsPlaying()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Countdown, 3f, false), Is.EqualTo(MatchPhase.Playing));
        }

        [Test]
        public void NextPhase_PlayingBeforeDuration_StaysPlaying()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Playing, 179.9f, false), Is.EqualTo(MatchPhase.Playing));
        }

        [Test]
        public void NextPhase_PlayingAtDuration_ShowsResults()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Playing, 180f, false), Is.EqualTo(MatchPhase.Results));
        }

        [Test]
        public void NextPhase_ResultsBeforeDuration_StaysResults()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Results, 7.9f, false), Is.EqualTo(MatchPhase.Results));
        }

        [Test]
        public void NextPhase_ResultsAtDuration_RestartsWithCountdown()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Results, 8f, false), Is.EqualTo(MatchPhase.Countdown));
        }

        [Test]
        public void NextPhase_ResultsWithRestartRequested_RestartsAtOnce()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Results, 0f, true), Is.EqualTo(MatchPhase.Countdown));
        }

        [Test]
        public void NextPhase_RestartRequestedOutsideResults_IsIgnored()
        {
            Assert.That(_rules.NextPhase(MatchPhase.Countdown, 0f, true), Is.EqualTo(MatchPhase.Countdown));
            Assert.That(_rules.NextPhase(MatchPhase.Playing, 0f, true), Is.EqualTo(MatchPhase.Playing));
        }

        [Test]
        public void NextPhase_ZeroResultsDuration_WaitsForRestartRequest()
        {
            var rules = MatchTestSettings.CreateRules(0f);

            Assert.That(rules.NextPhase(MatchPhase.Results, 1000f, false), Is.EqualTo(MatchPhase.Results));
            Assert.That(rules.NextPhase(MatchPhase.Results, 1000f, true), Is.EqualTo(MatchPhase.Countdown));
        }

        [Test]
        public void WaitsForRestartRequest_ResultsDuration_TrueOnlyWhenZero()
        {
            Assert.That(MatchTestSettings.CreateRules(8f).WaitsForRestartRequest, Is.False);
            Assert.That(MatchTestSettings.CreateRules(0f).WaitsForRestartRequest, Is.True);
        }

        [Test]
        public void StartsNextMatch_ResultsToCountdown_IsTrue()
        {
            Assert.That(MatchRules.StartsNextMatch(MatchPhase.Results, MatchPhase.Countdown), Is.True);
        }

        [Test]
        public void StartsNextMatch_OtherTransitions_AreFalse()
        {
            Assert.That(MatchRules.StartsNextMatch(MatchPhase.Countdown, MatchPhase.Countdown), Is.False);
            Assert.That(MatchRules.StartsNextMatch(MatchPhase.Countdown, MatchPhase.Playing), Is.False);
            Assert.That(MatchRules.StartsNextMatch(MatchPhase.Playing, MatchPhase.Results), Is.False);
        }

        [Test]
        public void IsInputLocked_EachPhase_UnlockedOnlyWhilePlaying()
        {
            Assert.That(MatchRules.IsInputLocked(MatchPhase.Countdown), Is.True);
            Assert.That(MatchRules.IsInputLocked(MatchPhase.Playing), Is.False);
            Assert.That(MatchRules.IsInputLocked(MatchPhase.Results), Is.True);
        }
    }
}
