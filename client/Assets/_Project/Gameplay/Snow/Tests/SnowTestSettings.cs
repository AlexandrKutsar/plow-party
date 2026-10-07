using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    internal static class SnowTestSettings
    {
        public const int Seed = 7;

        public static SnowSettings Create()
        {
            return new SnowSettings
            {
                Origin = Vector2.zero,
                Size = new Vector2(4f, 2f),
                CellSize = 0.5f,
                FullDepth = 3,
                RegrowthDelay = 0f,
                RegrowthStepInterval = 0f,
                PileStepsPerScrape = int.MaxValue,
                PileSpeedPenalty = 0.4f,
                BlizzardTimes = new float[0],
                BlizzardDuration = 2f,
                BladeWidth = 1f,
                BladeDepth = 0.4f,
                BladeForwardOffset = 0.75f,
            };
        }
    }
}
