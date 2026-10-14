namespace PlowParty.Meta.Party.Simulation
{
    public readonly struct PartyLooks
    {
        public const int DefaultColorCount = 8;
        public const int DefaultCritterCount = 6;

        public PartyLooks(int colorCount, int critterCount)
        {
            ColorCount = colorCount;
            CritterCount = critterCount;
        }

        public static PartyLooks Default => new PartyLooks(DefaultColorCount, DefaultCritterCount);

        public int ColorCount { get; }

        public int CritterCount { get; }
    }
}
