namespace PlowParty.Gameplay.Match.Simulation
{
    public readonly struct MatchPlacement
    {
        public MatchPlacement(int slot, int score, int place)
        {
            Slot = slot;
            Score = score;
            Place = place;
        }

        public int Slot { get; }

        public int Score { get; }

        public int Place { get; }
    }
}
