using PlowParty.Gameplay.Vehicle.Simulation;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    internal static class VehicleTestSettings
    {
        public const float Tick = 1f / 60f;

        public static VehicleSettings Create()
        {
            return new VehicleSettings
            {
                Radius = 0.5f,
                MaxSpeed = 10f,
                Acceleration = 20f,
                Deceleration = 10f,
                TurnRateDegrees = 180f,
                Restitution = 0.8f,
                RamMinAngleDegrees = 60f,
                RamMinSpeed = 3f,
                RamCooldown = 1f,
                RamKnockback = 4f,
                RamRecoil = 2f,
            };
        }
    }
}
