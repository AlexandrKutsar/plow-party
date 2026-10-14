namespace PlowParty.Meta.Lobby.Simulation
{
    public readonly struct ColorChoice
    {
        public ColorChoice(int color, bool isMine, bool isTaken)
        {
            Color = color;
            IsMine = isMine;
            IsTaken = isTaken;
        }

        public int Color { get; }

        public bool IsMine { get; }

        public bool IsTaken { get; }
    }
}
