using UnityEngine;

namespace PlowParty.Gameplay.Hud.Simulation
{
    public static class EdgeArrow
    {
        public static bool TryPlace(Vector3 screenPoint, Vector2 screenSize, float margin, out Vector2 position, out float angleDegrees)
        {
            var centre = screenSize * 0.5f;
            var direction = new Vector2(screenPoint.x, screenPoint.y) - centre;
            var inFront = screenPoint.z > 0f;
            if (inFront && IsInside(screenPoint, screenSize, margin))
            {
                position = default;
                angleDegrees = 0f;
                return false;
            }

            if (!inFront)
            {
                direction = -direction;
            }

            if (direction.sqrMagnitude < 1e-6f)
            {
                direction = Vector2.down;
            }

            var halfExtents = Vector2.Max(centre - new Vector2(margin, margin), Vector2.zero);
            var scale = Mathf.Min(ScaleTo(halfExtents.x, direction.x), ScaleTo(halfExtents.y, direction.y));
            position = centre + direction * scale;
            angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return true;
        }

        private static bool IsInside(Vector3 point, Vector2 size, float margin)
        {
            return point.x >= margin && point.x <= size.x - margin && point.y >= margin && point.y <= size.y - margin;
        }

        private static float ScaleTo(float halfExtent, float component)
        {
            var magnitude = Mathf.Abs(component);
            return magnitude < 1e-6f ? float.MaxValue : halfExtent / magnitude;
        }
    }
}
