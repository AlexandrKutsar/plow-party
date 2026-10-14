namespace PlowParty.Meta.Party.Simulation
{
    public readonly struct PartyMember
    {
        public PartyMember(int id, string nickname, bool isReady, int slot = -1, int color = -1, int critter = 0, string key = null)
        {
            Id = id;
            Nickname = nickname ?? string.Empty;
            IsReady = isReady;
            Slot = slot;
            Color = color;
            Critter = critter;
            Key = key ?? string.Empty;
        }

        public int Id { get; }

        public string Nickname { get; }

        public bool IsReady { get; }

        public int Slot { get; }

        public int Color { get; }

        public int Critter { get; }

        public string Key { get; }

        public PartyMember WithReady(bool isReady)
        {
            return new PartyMember(Id, Nickname, isReady, Slot, Color, Critter, Key);
        }

        public PartyMember WithSlot(int slot)
        {
            return new PartyMember(Id, Nickname, IsReady, slot, Color, Critter, Key);
        }

        public PartyMember WithColor(int color)
        {
            return new PartyMember(Id, Nickname, IsReady, Slot, color, Critter, Key);
        }

        public PartyMember WithCritter(int critter)
        {
            return new PartyMember(Id, Nickname, IsReady, Slot, Color, critter, Key);
        }
    }
}
