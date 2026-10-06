namespace PlowParty.Gameplay.DropOff.Simulation
{
    public readonly struct DeliveryTick
    {
        public static readonly DeliveryTick Idle = default;

        public DeliveryTick(Delivery next, int unloadedSteps, int scoreGained)
        {
            Next = next;
            UnloadedSteps = unloadedSteps;
            ScoreGained = scoreGained;
        }

        public Delivery Next { get; }

        public int UnloadedSteps { get; }

        public int ScoreGained { get; }
    }
}
