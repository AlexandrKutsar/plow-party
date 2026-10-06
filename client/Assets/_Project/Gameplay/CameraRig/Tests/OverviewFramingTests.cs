using NUnit.Framework;
using PlowParty.Gameplay.CameraRig.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Tests
{
    public sealed class OverviewFramingTests
    {
        private const float WideAspect = 16f / 9f;
        private const float Tolerance = 1e-3f;

        private static readonly Rect SmallArena = Rect.MinMaxRect(-15f, -15f, 15f, 15f);
        private static readonly Rect LargeArena = Rect.MinMaxRect(-20f, -20f, 20f, 20f);

        [Test]
        public void Fit_Perspective_KeepsEveryCornerOnScreen()
        {
            var pose = OverviewFraming.Fit(SmallArena, 0f, CameraTestSettings.PerspectiveOverview(), WideAspect);

            Assert.That(LargestScreenExtent(pose, SmallArena, WideAspect), Is.LessThanOrEqualTo(1f + Tolerance));
        }

        [Test]
        public void Fit_Perspective_TouchesTheFrustumOnTheLimitingSide()
        {
            var pose = OverviewFraming.Fit(SmallArena, 0f, CameraTestSettings.PerspectiveOverview(), WideAspect);

            Assert.That(LargestScreenExtent(pose, SmallArena, WideAspect), Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Fit_LargerArena_MovesCameraFarther()
        {
            var settings = CameraTestSettings.PerspectiveOverview();

            var small = OverviewFraming.Fit(SmallArena, 0f, settings, WideAspect);
            var large = OverviewFraming.Fit(LargeArena, 0f, settings, WideAspect);

            Assert.That(large.Position.magnitude, Is.GreaterThan(small.Position.magnitude));
        }

        [Test]
        public void Fit_Perspective_UsesConfiguredPitch()
        {
            var pose = OverviewFraming.Fit(SmallArena, 0f, CameraTestSettings.PerspectiveOverview(), WideAspect);

            Assert.That(pose.Rotation.eulerAngles.x, Is.EqualTo(60f).Within(Tolerance));
        }

        [Test]
        public void Fit_OrthographicWideScreen_SizeFitsArenaDepth()
        {
            var pose = OverviewFraming.Fit(SmallArena, 0f, CameraTestSettings.TopDownOrthographicOverview(0f), 2f);

            Assert.That(pose.Lens.OrthographicSize, Is.EqualTo(15f).Within(Tolerance));
        }

        [Test]
        public void Fit_OrthographicTallScreen_SizeFitsArenaWidth()
        {
            var pose = OverviewFraming.Fit(SmallArena, 0f, CameraTestSettings.TopDownOrthographicOverview(0f), 0.5f);

            Assert.That(pose.Lens.OrthographicSize, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void Fit_Padding_AddsMarginAroundArena()
        {
            var pose = OverviewFraming.Fit(SmallArena, 0f, CameraTestSettings.TopDownOrthographicOverview(1f), 2f);

            Assert.That(pose.Lens.OrthographicSize, Is.EqualTo(16f).Within(Tolerance));
        }

        [Test]
        public void Fit_Orthographic_LooksAtArenaCentreFromConfiguredDistance()
        {
            var arena = Rect.MinMaxRect(0f, 10f, 30f, 40f);

            var pose = OverviewFraming.Fit(arena, 2f, CameraTestSettings.TopDownOrthographicOverview(0f), 1f);

            Assert.That(Vector3.Distance(pose.Position, new Vector3(15f, 52f, 25f)), Is.LessThan(Tolerance));
        }

        private static float LargestScreenExtent(CameraPose pose, Rect arena, float aspect)
        {
            var tanVertical = Mathf.Tan(pose.Lens.FieldOfView * 0.5f * Mathf.Deg2Rad);
            var tanHorizontal = tanVertical * aspect;
            var inverse = Quaternion.Inverse(pose.Rotation);
            var largest = 0f;
            foreach (var corner in Corners(arena))
            {
                var local = inverse * (corner - pose.Position);
                Assert.That(local.z, Is.GreaterThan(0f));
                largest = Mathf.Max(largest, Mathf.Abs(local.x) / local.z / tanHorizontal);
                largest = Mathf.Max(largest, Mathf.Abs(local.y) / local.z / tanVertical);
            }

            return largest;
        }

        private static Vector3[] Corners(Rect arena)
        {
            return new[]
            {
                new Vector3(arena.xMin, 0f, arena.yMin),
                new Vector3(arena.xMax, 0f, arena.yMin),
                new Vector3(arena.xMin, 0f, arena.yMax),
                new Vector3(arena.xMax, 0f, arena.yMax),
            };
        }
    }
}
