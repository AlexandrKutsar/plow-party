namespace PlowParty.Meta.Party.Simulation
{
    public readonly struct PartyMember
    {
        public PartyMember(int id, string nickname, bool isReady)
        {
            Id = id;
            Nickname = nickname ?? string.Empty;
            IsReady = isReady;
        }

        public int Id { get; }

        public string Nickname { get; }

        public bool IsReady { get; }

        public PartyMember WithReady(bool isReady)
        {
            return new PartyMember(Id, Nickname, isReady);
        }
    }
}
