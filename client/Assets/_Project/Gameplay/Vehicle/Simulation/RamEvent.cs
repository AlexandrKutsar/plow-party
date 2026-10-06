namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public readonly struct RamEvent
    {
        public RamEvent(int rammer, int victim, float strength)
        {
            Rammer = rammer;
            Victim = victim;
            Strength = strength;
        }

        public int Rammer { get; }

        public int Victim { get; }

        public float Strength { get; }
    }
}
