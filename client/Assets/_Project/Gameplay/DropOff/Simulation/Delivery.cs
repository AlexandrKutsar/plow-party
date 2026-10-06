namespace PlowParty.Gameplay.DropOff.Simulation
{
    public readonly struct Delivery
    {
        public static readonly Delivery None = default;

        public Delivery(float multiplier, int deliveredSteps, float elapsed)
        {
            Multiplier = multiplier;
            DeliveredSteps = deliveredSteps;
            Elapsed = elapsed;
        }

        public float Multiplier { get; }

        public int DeliveredSteps { get; }

        public float Elapsed { get; }

        public bool IsActive => Multiplier > 0f;
    }
}
