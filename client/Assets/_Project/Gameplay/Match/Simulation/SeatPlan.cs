namespace PlowParty.Gameplay.Match.Simulation
{
    public readonly struct SeatPlan
    {
        public SeatPlan(int slotCount, int expectedPlayers)
        {
            SlotCount = slotCount;
            ExpectedPlayers = expectedPlayers;
        }

        public int SlotCount { get; }

        public int ExpectedPlayers { get; }

        public int PlannedBots => SlotCount - ExpectedPlayers;
    }
}
