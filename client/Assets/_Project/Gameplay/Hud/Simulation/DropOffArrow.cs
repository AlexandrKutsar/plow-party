using UnityEngine;

namespace PlowParty.Gameplay.Hud.Simulation
{
    public static class DropOffArrow
    {
        public static bool TryPlace(Vector3 screenPoint, Rect bounds, float margin, out Vector2 position, out float angleDegrees)
        {
            var inner = Shrink(bounds, margin);
            var centre = inner.center;
            var direction = new Vector2(screenPoint.x, screenPoint.y) - centre;
            var inFront = screenPoint.z > 0f;
            if (inFront && inner.Contains(new Vector2(screenPoint.x, screenPoint.y)))
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

            var scale = Mathf.Min(ScaleTo(inner.width * 0.5f, direction.x), ScaleTo(inner.height * 0.5f, direction.y));
            position = centre + direction * scale;
            angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return true;
        }

        private static Rect Shrink(Rect bounds, float margin)
        {
            var width = Mathf.Max(0f, bounds.width - 2f * margin);
            var height = Mathf.Max(0f, bounds.height - 2f * margin);
            return new Rect(bounds.center.x - width * 0.5f, bounds.center.y - height * 0.5f, width, height);
        }

        private static float ScaleTo(float halfExtent, float component)
        {
            var magnitude = Mathf.Abs(component);
            return magnitude < 1e-6f ? float.MaxValue : halfExtent / magnitude;
        }
    }
}
