using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class SegmentMathTests
    {
        [Test]
        public void ClosestPoints_CrossingSegments_MeetAtIntersection()
        {
            SegmentMath.ClosestPoints(new Vector2(-1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, -1f), new Vector2(0.5f, 1f), out var onFirst, out var onSecond);

            Assert.That(onFirst, Is.EqualTo(new Vector2(0.5f, 0f)));
            Assert.That(onSecond, Is.EqualTo(new Vector2(0.5f, 0f)));
        }

        [Test]
        public void ClosestPoints_ParallelOverlappingSegments_AreSideBySide()
        {
            SegmentMath.ClosestPoints(new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(1f, 1f), new Vector2(1f, 3f), out var onFirst, out var onSecond);

            Assert.That(Vector2.Distance(onFirst, onSecond), Is.EqualTo(1f).Within(1e-5f));
            Assert.That(onFirst.y, Is.EqualTo(onSecond.y).Within(1e-5f));
        }

        [Test]
        public void ClosestPoints_SegmentsEndToEnd_JoinNearestEnds()
        {
            SegmentMath.ClosestPoints(new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(3f, 0f), new Vector2(2f, 0f), out var onFirst, out var onSecond);

            Assert.That(onFirst, Is.EqualTo(new Vector2(1f, 0f)));
            Assert.That(onSecond, Is.EqualTo(new Vector2(2f, 0f)));
        }

        [Test]
        public void ClosestPointsToBox_SegmentPassingCorner_PairsCornerWithSegment()
        {
            var box = new BoxObstacle(new Vector2(2f, 2f), new Vector2(1f, 1f));

            SegmentMath.ClosestPoints(new Vector2(0f, 1.5f), new Vector2(1.5f, 0f), box, out var onSegment, out var onBox);

            Assert.That(Vector2.Distance(onSegment, new Vector2(0.75f, 0.75f)), Is.LessThan(1e-5f));
            Assert.That(onBox, Is.EqualTo(new Vector2(1f, 1f)));
        }

        [Test]
        public void ClosestPointsToBox_SegmentThroughBox_PicksDeepestPointInside()
        {
            var box = new BoxObstacle(Vector2.zero, new Vector2(1f, 0.5f));

            SegmentMath.ClosestPoints(new Vector2(-3f, 0.1f), new Vector2(3f, 0.1f), box, out var onSegment, out var onBox);

            Assert.That(onSegment, Is.EqualTo(onBox));
            Assert.That(SegmentMath.DepthInsideBox(onSegment, box), Is.EqualTo(0.4f).Within(1e-5f));
        }
    }
}
