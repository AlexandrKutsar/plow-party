using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public readonly struct VehicleModifiers
    {
        public VehicleModifiers(float speedMultiplier, bool isImmobilised, Vector2 impulse, float ramStrengthMultiplier)
        {
            SpeedMultiplier = speedMultiplier;
            IsImmobilised = isImmobilised;
            Impulse = impulse;
            RamStrengthMultiplier = ramStrengthMultiplier;
        }

        public float SpeedMultiplier { get; }

        public bool IsImmobilised { get; }

        public Vector2 Impulse { get; }

        public float RamStrengthMultiplier { get; }

        public VehicleModifiers WithoutImpulse()
        {
            return new VehicleModifiers(SpeedMultiplier, IsImmobilised, Vector2.zero, RamStrengthMultiplier);
        }

        public static VehicleModifiers None => new VehicleModifiers(1f, false, Vector2.zero, 1f);
    }
}
