namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public sealed class VehicleSettings
    {
        public float Radius { get; set; }

        public float HalfLength { get; set; }

        public float MaxSpeed { get; set; }

        public float Acceleration { get; set; }

        public float Deceleration { get; set; }

        public float TurnRateDegrees { get; set; }

        public float Restitution { get; set; }

        public float RamMinAngleDegrees { get; set; }

        public float RamMinSpeed { get; set; }

        public float RamCooldown { get; set; }

        public float RamKnockback { get; set; }

        public float RamRecoil { get; set; }
    }
}
