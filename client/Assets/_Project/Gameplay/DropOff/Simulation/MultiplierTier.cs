namespace PlowParty.Gameplay.DropOff.Simulation
{
    public readonly struct MultiplierTier
    {
        public MultiplierTier(int minLoad, float multiplier)
        {
            MinLoad = minLoad;
            Multiplier = multiplier;
        }

        public int MinLoad { get; }

        public float Multiplier { get; }
    }
}
