namespace PlowParty.Gameplay.Bucket.Simulation
{
    public sealed class BucketSettings
    {
        public int Capacity { get; set; }

        public int StepsPerLoad { get; set; }

        public float MaxSpeedPenalty { get; set; }

        public float SpillShare { get; set; }
    }
}
