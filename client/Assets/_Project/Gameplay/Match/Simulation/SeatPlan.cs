namespace PlowParty.Gameplay.Match.Simulation
{
    public readonly struct SeatPlan
    {
        public SeatPlan(int slotCount, int expectedPlayers, bool replacesMissingPlayers = true)
        {
            SlotCount = slotCount;
            ExpectedPlayers = expectedPlayers;
            ReplacesMissingPlayers = replacesMissingPlayers;
        }

        public int SlotCount { get; }

        public int ExpectedPlayers { get; }

        public bool ReplacesMissingPlayers { get; }

        public int PlannedBots => SlotCount - ExpectedPlayers;
    }
}
