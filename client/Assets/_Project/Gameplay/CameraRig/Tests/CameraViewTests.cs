using NUnit.Framework;
using PlowParty.Gameplay.CameraRig.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Tests
{
    public sealed class CameraViewTests
    {
        private const float Tolerance = 1e-3f;

        private static readonly CameraLens Perspective = new CameraLens(false, 40f, 0f);

        [Test]
        public void PoseAt_TiltedView_PlacesCameraAtDistanceAboveFocus()
        {
            var view = new CameraView(55f, 0f, 14f, Perspective);
            var focus = new Vector3(3f, 0f, 4f);

            var pose = view.PoseAt(focus);

            Assert.That(Vector3.Distance(pose.Position, focus), Is.EqualTo(14f).Within(Tolerance));
            Assert.That(pose.Position.y, Is.EqualTo(14f * Mathf.Sin(55f * Mathf.Deg2Rad)).Within(Tolerance));
        }

        [Test]
        public void PoseAt_NorthUp_PlacesCameraSouthOfFocus()
        {
            var view = new CameraView(55f, 0f, 14f, Perspective);

            var pose = view.PoseAt(Vector3.zero);

            Assert.That(pose.Position.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(pose.Position.z, Is.LessThan(0f));
        }

        [Test]
        public void GroundFootprint_TopDownOrthographic_IsTheScreenRectangle()
        {
            var view = new CameraView(90f, 0f, 20f, new CameraLens(true, 40f, 5f));

            var footprint = view.GroundFootprint(2f);

            Assert.That(footprint.xMin, Is.EqualTo(-10f).Within(Tolerance));
            Assert.That(footprint.xMax, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(footprint.yMin, Is.EqualTo(-5f).Within(Tolerance));
            Assert.That(footprint.yMax, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void GroundFootprint_TiltedPerspective_IsSymmetricSideways()
        {
            var view = new CameraView(55f, 0f, 14f, Perspective);

            var footprint = view.GroundFootprint(16f / 9f);

            Assert.That(footprint.xMin, Is.EqualTo(-footprint.xMax).Within(Tolerance));
        }

        [Test]
        public void GroundFootprint_TiltedPerspective_ReachesFartherAheadThanBehind()
        {
            var view = new CameraView(55f, 0f, 14f, Perspective);

            var footprint = view.GroundFootprint(16f / 9f);

            Assert.That(footprint.yMax, Is.GreaterThan(-footprint.yMin));
        }

        [Test]
        public void GroundFootprint_FacingEast_ReachesFartherEast()
        {
            var view = new CameraView(55f, 90f, 14f, Perspective);

            var footprint = view.GroundFootprint(16f / 9f);

            Assert.That(footprint.xMax, Is.GreaterThan(-footprint.xMin));
        }
    }
}
