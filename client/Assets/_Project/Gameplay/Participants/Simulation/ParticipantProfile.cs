namespace PlowParty.Gameplay.Participants.Simulation
{
    public readonly struct ParticipantProfile
    {
        public ParticipantProfile(string nickname, CritterSpecies species, bool isBot)
        {
            Nickname = nickname;
            Species = species;
            IsBot = isBot;
        }

        public string Nickname { get; }

        public CritterSpecies Species { get; }

        public bool IsBot { get; }
    }
}
