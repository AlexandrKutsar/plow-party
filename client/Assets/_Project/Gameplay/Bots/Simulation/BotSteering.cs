using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public static class BotSteering
    {
        private const float SharpTurnDegrees = 120f;
        private const float WideTurnDegrees = 60f;
        private const float SharpTurnThrottle = 0.35f;
        private const float WideTurnThrottle = 0.65f;

        public static Vector2 Steer(
            BotVehicle self,
            Vector2 aim,
            BotVehicle[] vehicles,
            int count,
            int ignoredSlot,
            BotSettings settings,
            float noiseDegrees,
            float throttleCap)
        {
            var toAim = aim - self.Position;
            if (toAim.sqrMagnitude <= 0f)
            {
                return Vector2.zero;
            }

            var desired = toAim.normalized + Avoidance(self, vehicles, count, ignoredSlot, settings);
            if (desired.sqrMagnitude <= 0f)
            {
                desired = toAim.normalized;
            }

            desired = Rotate(desired.normalized, noiseDegrees);
            return desired * (Throttle(Vector2.Angle(self.Forward, desired)) * throttleCap);
        }

        private static Vector2 Avoidance(BotVehicle self, BotVehicle[] vehicles, int count, int ignoredSlot, BotSettings settings)
        {
            var push = Vector2.zero;
            for (var i = 0; i < count; i++)
            {
                var other = vehicles[i];
                if (other.Slot == self.Slot || other.Slot == ignoredSlot)
                {
                    continue;
                }

                var away = self.Position - other.Position;
                var distance = away.magnitude;
                if (distance <= 0f || distance >= settings.AvoidRadius || Vector2.Dot(self.Forward, -away) <= 0f)
                {
                    continue;
                }

                push += away / distance * (settings.AvoidStrength * (1f - distance / settings.AvoidRadius));
            }

            return push;
        }

        private static float Throttle(float turnDegrees)
        {
            if (turnDegrees > SharpTurnDegrees)
            {
                return SharpTurnThrottle;
            }

            return turnDegrees > WideTurnDegrees ? WideTurnThrottle : 1f;
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos);
        }
    }
}
