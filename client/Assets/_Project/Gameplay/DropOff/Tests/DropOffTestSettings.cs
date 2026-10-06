using PlowParty.Gameplay.Bucket.Simulation;
using PlowParty.Gameplay.DropOff.Simulation;

namespace PlowParty.Gameplay.DropOff.Tests
{
    internal static class DropOffTestSettings
    {
        public const int StepsPerLoad = 2;
        public const int FullBucketSteps = 200;

        public static DropOffSettings Create()
        {
            return new DropOffSettings
            {
                ZoneRadius = 3f,
                SnowFreeRadius = 4f,
                FullUnloadDuration = 2f,
                BaseMultiplier = 1f,
                MultiplierTiers = new[]
                {
                    new MultiplierTier(51, 1.5f),
                    new MultiplierTier(100, 2f),
                },
            };
        }

        public static BucketSettings CreateBucket()
        {
            return new BucketSettings
            {
                Capacity = 100,
                StepsPerLoad = StepsPerLoad,
                MaxSpeedPenalty = 0.25f,
                SpillShare = 0.3f,
            };
        }

        public static DropOffRules CreateRules()
        {
            return new DropOffRules(Create(), CreateBucket());
        }
    }
}
