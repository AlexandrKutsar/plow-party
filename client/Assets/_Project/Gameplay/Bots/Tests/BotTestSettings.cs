using PlowParty.Gameplay.Bots.Simulation;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Tests
{
    internal static class BotTestSettings
    {
        public static readonly Vector2 Origin = new Vector2(-10f, -10f);
        public static readonly Vector2 Size = new Vector2(20f, 20f);

        public static BotSettings Create()
        {
            return new BotSettings
            {
                NavCellSize = 1f,
                NavClearance = 0.7f,
                WaypointReach = 0.9f,
                ReplanInterval = 0.5f,
                AvoidRadius = 2.5f,
                AvoidStrength = 1.5f,
                StuckDistance = 0.5f,
                StuckTime = 1f,
                UnstuckDuration = 0.7f,
                CruiseSpeed = 6f,
                SnowTargetReach = 1.5f,
                SnowFalloff = 8f,
                CrowdRadius = 3f,
                CrowdPenalty = 0.5f,
                MinPileSteps = 20,
                PileFalloff = 12f,
                RamRange = 10f,
                RamMinVictimLoad = 20,
                RamFalloff = 6f,
                RamLeadTime = 0.3f,
                ThreatRange = 6f,
                ThreatClosingSpeed = 3f,
                ThreatConeDegrees = 35f,
                EvadeDistance = 5f,
                DropOffStandDepth = 1f,
                DeliveryMargin = 4f,
                GreedReach = 15,
                CollectWeight = 1f,
                DeliverWeight = 1f,
                PileWeight = 1.3f,
                RamWeight = 1.2f,
                EvadeWeight = 1.2f,
                CommitmentBonus = 0.15f,
                WeakShare = 0.5f,
            };
        }

        public static BotProfile Profile(BotDifficulty difficulty = BotDifficulty.Strong)
        {
            return new BotProfile
            {
                Difficulty = difficulty,
                ReactionDelay = 0.1f,
                DecisionIntervalMin = 0.2f,
                DecisionIntervalMax = 0.3f,
                SteeringNoiseDegrees = 0f,
                MistakeChance = 0f,
                Aggression = 1f,
                Greed = 1f,
                Caution = 1f,
                ThrottleCap = 1f,
            };
        }

        public static NavGrid Grid(VehicleArena arena)
        {
            return NavGrid.Build(arena, Origin, Size, 1f, 0.7f);
        }

        public static NavGrid OpenGrid()
        {
            return Grid(VehicleArena.Create());
        }
    }
}
