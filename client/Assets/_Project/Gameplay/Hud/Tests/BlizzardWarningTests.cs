using NUnit.Framework;
using PlowParty.Gameplay.Hud.Simulation;

namespace PlowParty.Gameplay.Hud.Tests
{
    public sealed class BlizzardWarningTests
    {
        private BlizzardWarning _warning;

        [SetUp]
        public void SetUp()
        {
            _warning = new BlizzardWarning(new[] { 45f, 90f, 135f }, 5f);
        }

        [Test]
        public void TryGetSecondsLeft_BeforeWarningWindow_IsFalse()
        {
            Assert.That(_warning.TryGetSecondsLeft(39.9f, out _), Is.False);
        }

        [Test]
        public void TryGetSecondsLeft_InsideWarningWindow_ReturnsSecondsToWave()
        {
            Assert.That(_warning.TryGetSecondsLeft(42f, out var secondsLeft), Is.True);
            Assert.That(secondsLeft, Is.EqualTo(3f).Within(1e-4f));
        }

        [Test]
        public void TryGetSecondsLeft_ExactlyLeadBeforeWave_IsTrue()
        {
            Assert.That(_warning.TryGetSecondsLeft(85f, out var secondsLeft), Is.True);
            Assert.That(secondsLeft, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void TryGetSecondsLeft_AtWaveStart_IsFalse()
        {
            Assert.That(_warning.TryGetSecondsLeft(135f, out _), Is.False);
        }

        [Test]
        public void TryGetSecondsLeft_AfterLastWave_IsFalse()
        {
            Assert.That(_warning.TryGetSecondsLeft(170f, out _), Is.False);
        }

        [Test]
        public void TryGetSecondsLeft_NoWaves_IsFalse()
        {
            Assert.That(new BlizzardWarning(new float[0], 5f).TryGetSecondsLeft(10f, out _), Is.False);
        }
    }
}
