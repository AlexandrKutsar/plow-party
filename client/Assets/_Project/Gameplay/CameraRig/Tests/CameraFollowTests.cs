using NUnit.Framework;
using PlowParty.Gameplay.CameraRig.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Tests
{
    public sealed class CameraFollowTests
    {
        private const float Aspect = 1f;
        private const float Frame = 1f / 60f;
        private const float Tolerance = 1e-3f;

        private CameraFollow _follow;
        private FollowSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _follow = new CameraFollow();
            _settings = CameraTestSettings.TopDownFollow();
        }

        [Test]
        public void Step_FirstTarget_SnapsOntoTarget()
        {
            Step(CameraTestSettings.StillTarget(new Vector2(3f, 4f)));

            AssertFocus(new Vector2(3f, 4f));
        }

        [Test]
        public void Step_SmallMove_LagsBehindTarget()
        {
            Step(CameraTestSettings.StillTarget(Vector2.zero));

            Step(CameraTestSettings.StillTarget(new Vector2(1f, 0f)));

            Assert.That(_follow.Focus.x, Is.GreaterThan(0f).And.LessThan(1f));
        }

        [Test]
        public void Step_TargetHoldsStill_FocusConvergesOnIt()
        {
            Step(CameraTestSettings.StillTarget(Vector2.zero));

            for (var frame = 0; frame < 300; frame++)
            {
                Step(CameraTestSettings.StillTarget(new Vector2(2f, 1f)));
            }

            AssertFocus(new Vector2(2f, 1f));
        }

        [Test]
        public void Step_TeleportBeyondSnapDistance_SnapsAtOnce()
        {
            Step(CameraTestSettings.StillTarget(Vector2.zero));

            Step(CameraTestSettings.StillTarget(new Vector2(10f, -8f)));

            AssertFocus(new Vector2(10f, -8f));
        }

        [Test]
        public void Step_Moving_LooksAheadAlongVelocity()
        {
            Step(new CameraTarget(Vector2.zero, 0f, new Vector2(4f, 0f), Vector2.right));

            AssertFocus(new Vector2(2f, 0f));
        }

        [Test]
        public void Step_MovingFast_CapsLookAhead()
        {
            Step(new CameraTarget(Vector2.zero, 0f, new Vector2(0f, 100f), Vector2.up));

            AssertFocus(new Vector2(0f, 3f));
        }

        [Test]
        public void Step_NearArenaEdge_StopsAtOverscan()
        {
            Step(CameraTestSettings.StillTarget(new Vector2(19f, -19f)));

            AssertFocus(new Vector2(16f, -16f));
        }

        [Test]
        public void Step_ArenaNarrowerThanView_CentresOnArena()
        {
            var narrow = Rect.MinMaxRect(-3f, -20f, 3f, 20f);

            _follow.Step(CameraTestSettings.StillTarget(new Vector2(2f, 6f)), _settings, narrow, Aspect, Frame);

            AssertFocus(new Vector2(0f, 6f));
        }

        [Test]
        public void Step_FixedOrientation_KeepsConfiguredYaw()
        {
            var pose = Step(new CameraTarget(Vector2.zero, 0f, Vector2.zero, Vector2.right));

            Assert.That(_follow.Yaw, Is.EqualTo(0f).Within(Tolerance));
            Assert.That((pose.Rotation * Vector3.up).z, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Step_RotatingFirstTarget_SnapsToHeading()
        {
            _settings = CameraTestSettings.TopDownRotatingFollow();

            Step(new CameraTarget(Vector2.zero, 0f, Vector2.zero, Vector2.right));

            Assert.That(_follow.Yaw, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void Step_RotatingTurn_EasesTowardHeading()
        {
            _settings = CameraTestSettings.TopDownRotatingFollow();
            Step(new CameraTarget(Vector2.zero, 0f, Vector2.zero, Vector2.up));

            Step(new CameraTarget(Vector2.zero, 0f, Vector2.zero, Vector2.right));

            Assert.That(_follow.Yaw, Is.GreaterThan(0f).And.LessThan(90f));
        }

        [Test]
        public void Step_RotatingWithoutHeading_KeepsYaw()
        {
            _settings = CameraTestSettings.TopDownRotatingFollow();
            Step(new CameraTarget(Vector2.zero, 0f, Vector2.zero, Vector2.right));

            Step(new CameraTarget(Vector2.zero, 0f, Vector2.zero, Vector2.zero));

            Assert.That(_follow.Yaw, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void Step_TargetHeight_RaisesFocus()
        {
            var pose = Step(new CameraTarget(Vector2.zero, 2f, Vector2.zero, Vector2.up));

            Assert.That(pose.Position.y, Is.EqualTo(2f + _settings.Distance).Within(Tolerance));
        }

        [Test]
        public void Release_ThenSmallMove_SnapsAtOnce()
        {
            Step(CameraTestSettings.StillTarget(Vector2.zero));
            _follow.Release();

            Step(CameraTestSettings.StillTarget(new Vector2(1f, 0f)));

            AssertFocus(new Vector2(1f, 0f));
        }

        private CameraPose Step(CameraTarget target)
        {
            return _follow.Step(target, _settings, CameraTestSettings.Arena, Aspect, Frame);
        }

        private void AssertFocus(Vector2 expected)
        {
            Assert.That(_follow.Focus.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(_follow.Focus.y, Is.EqualTo(expected.y).Within(Tolerance));
        }
    }
}
