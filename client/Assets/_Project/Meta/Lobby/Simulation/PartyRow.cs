namespace PlowParty.Meta.Lobby.Simulation
{
    public readonly struct PartyRow
    {
        public PartyRow(int memberId, string nickname, bool hasCrown, string readyText, bool canRemove)
        {
            MemberId = memberId;
            Nickname = nickname;
            HasCrown = hasCrown;
            ReadyText = readyText;
            CanRemove = canRemove;
        }

        public int MemberId { get; }

        public string Nickname { get; }

        public bool HasCrown { get; }

        public string ReadyText { get; }

        public bool CanRemove { get; }
    }
}
