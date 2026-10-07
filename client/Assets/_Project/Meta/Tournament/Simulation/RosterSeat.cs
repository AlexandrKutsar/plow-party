namespace PlowParty.Meta.Tournament.Simulation
{
    public readonly struct RosterSeat
    {
        public RosterSeat(int slot, string accountId)
        {
            Slot = slot;
            AccountId = accountId;
        }

        public int Slot { get; }

        public string AccountId { get; }
    }
}
