using NUnit.Framework;
using PlowParty.Gameplay.CameraRig.Simulation;

namespace PlowParty.Gameplay.CameraRig.Tests
{
    public sealed class CameraShakeTests
    {
        private const float Tolerance = 1e-4f;

        private CameraShake _shake;

        [SetUp]
        public void SetUp()
        {
            _shake = new CameraShake();
        }

        [Test]
        public void Add_PastOne_ClampsTrauma()
        {
            _shake.Add(0.7f);
            _shake.Add(0.7f);

            Assert.That(_shake.Trauma, Is.EqualTo(1f));
        }

        [Test]
        public void Add_Negative_IsIgnored()
        {
            _shake.Add(0.5f);
            _shake.Add(-1f);

            Assert.That(_shake.Trauma, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void Step_NoTrauma_GivesNoOffsetOrRoll()
        {
            var sample = _shake.Step(0.1f, CameraTestSettings.Shake());

            Assert.That(sample.Offset.magnitude, Is.EqualTo(0f));
            Assert.That(sample.Roll, Is.EqualTo(0f));
        }

        [Test]
        public void Step_WithTrauma_DecaysLinearly()
        {
            _shake.Add(1f);

            _shake.Step(0.25f, CameraTestSettings.Shake());

            Assert.That(_shake.Trauma, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void Step_LongEnough_StopsAtZero()
        {
            _shake.Add(1f);

            _shake.Step(5f, CameraTestSettings.Shake());

            Assert.That(_shake.Trauma, Is.EqualTo(0f));
        }

        [Test]
        public void Step_WithTrauma_StaysWithinSquaredTraumaLimits()
        {
            var settings = CameraTestSettings.Shake();
            settings.DecayPerSecond = 0f;
            _shake.Add(0.5f);

            for (var frame = 0; frame < 120; frame++)
            {
                var sample = _shake.Step(1f / 60f, settings);

                Assert.That(sample.Offset.x, Is.InRange(-0.125f - Tolerance, 0.125f + Tolerance));
                Assert.That(sample.Offset.y, Is.InRange(-0.125f - Tolerance, 0.125f + Tolerance));
                Assert.That(sample.Roll, Is.InRange(-0.75f - Tolerance, 0.75f + Tolerance));
            }
        }

        [Test]
        public void Step_WithTrauma_MovesTheCamera()
        {
            var settings = CameraTestSettings.Shake();
            settings.DecayPerSecond = 0f;
            _shake.Add(1f);
            var largest = 0f;

            for (var frame = 0; frame < 60; frame++)
            {
                largest = UnityEngine.Mathf.Max(largest, _shake.Step(1f / 60f, settings).Offset.magnitude);
            }

            Assert.That(largest, Is.GreaterThan(0.05f));
        }
    }
}
