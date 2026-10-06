namespace PlowParty.Gameplay.DropOff.Simulation
{
    public sealed class DropOffSettings
    {
        public float ZoneRadius { get; set; }

        public float SnowFreeRadius { get; set; }

        public float FullUnloadDuration { get; set; }

        public float BaseMultiplier { get; set; }

        public MultiplierTier[] MultiplierTiers { get; set; }
    }
}
