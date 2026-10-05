using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public static class SegmentMath
    {
        private const float Epsilon = 1e-8f;

        public static Vector2 ClosestPoint(Vector2 start, Vector2 end, Vector2 point)
        {
            var direction = end - start;
            var lengthSquared = direction.sqrMagnitude;
            if (lengthSquared <= Epsilon)
            {
                return start;
            }

            var t = Mathf.Clamp01(Vector2.Dot(point - start, direction) / lengthSquared);
            return start + direction * t;
        }

        public static void ClosestPoints(Vector2 firstStart, Vector2 firstEnd, Vector2 secondStart, Vector2 secondEnd, out Vector2 onFirst, out Vector2 onSecond)
        {
            var firstDirection = firstEnd - firstStart;
            var secondDirection = secondEnd - secondStart;
            var between = firstStart - secondStart;
            var firstLengthSquared = firstDirection.sqrMagnitude;
            var secondLengthSquared = secondDirection.sqrMagnitude;
            var secondProjection = Vector2.Dot(secondDirection, between);
            float s;
            float t;
            if (firstLengthSquared <= Epsilon && secondLengthSquared <= Epsilon)
            {
                s = 0f;
                t = 0f;
            }
            else if (firstLengthSquared <= Epsilon)
            {
                s = 0f;
                t = Mathf.Clamp01(secondProjection / secondLengthSquared);
            }
            else
            {
                var firstProjection = Vector2.Dot(firstDirection, between);
                if (secondLengthSquared <= Epsilon)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-firstProjection / firstLengthSquared);
                }
                else
                {
                    var cross = Vector2.Dot(firstDirection, secondDirection);
                    var denominator = firstLengthSquared * secondLengthSquared - cross * cross;
                    s = denominator > Epsilon ? Mathf.Clamp01((cross * secondProjection - firstProjection * secondLengthSquared) / denominator) : 0f;
                    t = (cross * s + secondProjection) / secondLengthSquared;
                    if (t < 0f)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-firstProjection / firstLengthSquared);
                    }
                    else if (t > 1f)
                    {
                        t = 1f;
                        s = Mathf.Clamp01((cross - firstProjection) / firstLengthSquared);
                    }
                }
            }

            onFirst = firstStart + firstDirection * s;
            onSecond = secondStart + secondDirection * t;
        }

        public static void ClosestPoints(Vector2 start, Vector2 end, BoxObstacle box, out Vector2 onSegment, out Vector2 onBox)
        {
            if (TryClip(start, end, box, out var enter, out var exit))
            {
                onSegment = DeepestPoint(start, end, enter, exit, box);
                onBox = onSegment;
                return;
            }

            var bestDistance = float.MaxValue;
            onSegment = start;
            onBox = start;
            Consider(start, ClampToBox(start, box), ref bestDistance, ref onSegment, ref onBox);
            Consider(end, ClampToBox(end, box), ref bestDistance, ref onSegment, ref onBox);
            var min = box.Center - box.HalfExtents;
            var max = box.Center + box.HalfExtents;
            ConsiderCorner(start, end, min, ref bestDistance, ref onSegment, ref onBox);
            ConsiderCorner(start, end, max, ref bestDistance, ref onSegment, ref onBox);
            ConsiderCorner(start, end, new Vector2(min.x, max.y), ref bestDistance, ref onSegment, ref onBox);
            ConsiderCorner(start, end, new Vector2(max.x, min.y), ref bestDistance, ref onSegment, ref onBox);
        }

        public static Vector2 ClampToBox(Vector2 point, BoxObstacle box)
        {
            var min = box.Center - box.HalfExtents;
            var max = box.Center + box.HalfExtents;
            return new Vector2(Mathf.Clamp(point.x, min.x, max.x), Mathf.Clamp(point.y, min.y, max.y));
        }

        public static float DepthInsideBox(Vector2 point, BoxObstacle box)
        {
            var local = point - box.Center;
            return Mathf.Min(box.HalfExtents.x - Mathf.Abs(local.x), box.HalfExtents.y - Mathf.Abs(local.y));
        }

        private static void ConsiderCorner(Vector2 start, Vector2 end, Vector2 corner, ref float bestDistance, ref Vector2 onSegment, ref Vector2 onBox)
        {
            Consider(ClosestPoint(start, end, corner), corner, ref bestDistance, ref onSegment, ref onBox);
        }

        private static void Consider(Vector2 segmentPoint, Vector2 boxPoint, ref float bestDistance, ref Vector2 onSegment, ref Vector2 onBox)
        {
            var distance = (segmentPoint - boxPoint).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                onSegment = segmentPoint;
                onBox = boxPoint;
            }
        }

        private static Vector2 DeepestPoint(Vector2 start, Vector2 end, float enter, float exit, BoxObstacle box)
        {
            var direction = end - start;
            var entry = start + direction * enter;
            var exitPoint = start + direction * exit;
            var best = Deeper(entry, exitPoint, box);
            return Deeper(best, ClosestPoint(entry, exitPoint, box.Center), box);
        }

        private static Vector2 Deeper(Vector2 first, Vector2 second, BoxObstacle box)
        {
            return DepthInsideBox(second, box) > DepthInsideBox(first, box) ? second : first;
        }

        private static bool TryClip(Vector2 start, Vector2 end, BoxObstacle box, out float enter, out float exit)
        {
            enter = 0f;
            exit = 1f;
            var direction = end - start;
            var min = box.Center - box.HalfExtents;
            var max = box.Center + box.HalfExtents;
            return ClipAxis(start.x, direction.x, min.x, max.x, ref enter, ref exit)
                && ClipAxis(start.y, direction.y, min.y, max.y, ref enter, ref exit);
        }

        private static bool ClipAxis(float origin, float direction, float min, float max, ref float enter, ref float exit)
        {
            if (Mathf.Abs(direction) <= Epsilon)
            {
                return origin >= min && origin <= max;
            }

            var near = (min - origin) / direction;
            var far = (max - origin) / direction;
            if (near > far)
            {
                (near, far) = (far, near);
            }

            enter = Mathf.Max(enter, near);
            exit = Mathf.Min(exit, far);
            return enter <= exit;
        }
    }
}
