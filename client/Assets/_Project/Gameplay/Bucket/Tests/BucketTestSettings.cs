using PlowParty.Gameplay.Bucket.Simulation;

namespace PlowParty.Gameplay.Bucket.Tests
{
    internal static class BucketTestSettings
    {
        public static BucketSettings Create()
        {
            return new BucketSettings
            {
                Capacity = 10,
                StepsPerLoad = 4,
                MaxSpeedPenalty = 0.2f,
                SpillShare = 0.3f,
            };
        }
    }
}
