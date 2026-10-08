namespace PlowParty.Gameplay.Participants.Simulation
{
    public readonly struct ParticipantProfile
    {
        public ParticipantProfile(string nickname, CritterSpecies species, int color, bool isBot)
        {
            Nickname = nickname;
            Species = species;
            Color = color;
            IsBot = isBot;
        }

        public string Nickname { get; }

        public CritterSpecies Species { get; }

        public int Color { get; }

        public bool IsBot { get; }
    }
}
