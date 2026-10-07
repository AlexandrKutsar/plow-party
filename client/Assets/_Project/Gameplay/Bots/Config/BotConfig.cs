using PlowParty.Gameplay.Bots.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Bot Config", fileName = nameof(BotConfig))]
    public sealed class BotConfig : ScriptableObject
    {
        [Header("Difficulty")]
        [SerializeField] private BotProfileConfig _strong;
        [SerializeField] private BotProfileConfig _medium;
        [SerializeField] private BotProfileConfig _weak;
        [SerializeField, Range(0f, 1f)] private float _weakShare = 0.5f;

        [Header("Navigation")]
        [SerializeField, Min(0.25f)] private float _navCellSize = 1f;
        [SerializeField, Min(0f)] private float _navClearance = 0.7f;
        [SerializeField, Min(0.1f)] private float _waypointReach = 0.9f;
        [SerializeField, Min(0f)] private float _replanInterval = 0.5f;
        [SerializeField, Min(0f)] private float _avoidRadius = 2.5f;
        [SerializeField, Min(0f)] private float _avoidStrength = 1.5f;
        [SerializeField, Min(0.05f)] private float _stuckDistance = 0.5f;
        [SerializeField, Min(0.1f)] private float _stuckTime = 1f;
        [SerializeField, Min(0.1f)] private float _unstuckDuration = 0.7f;
        [SerializeField, Min(1f)] private float _cruiseSpeed = 6f;
        [SerializeField, Min(0.5f)] private float _stuckReportTime = 3f;

        [Header("Perception")]
        [SerializeField, Min(0.1f)] private float _snowRefreshInterval = 0.25f;
        [SerializeField, Min(0.5f)] private float _snowTargetReach = 2f;
        [SerializeField, Min(0.5f)] private float _snowFalloff = 8f;
        [SerializeField, Min(0f)] private float _crowdRadius = 3f;
        [SerializeField, Range(0f, 1f)] private float _crowdPenalty = 0.5f;
        [SerializeField, Min(1)] private int _minPileSteps = 20;
        [SerializeField, Min(0.5f)] private float _pileFalloff = 12f;
        [SerializeField, Min(0f)] private float _ramRange = 10f;
        [SerializeField, Min(0)] private int _ramMinVictimLoad = 20;
        [SerializeField, Min(0.5f)] private float _ramFalloff = 6f;
        [SerializeField, Min(0f)] private float _ramLeadTime = 0.3f;
        [SerializeField, Min(0f)] private float _threatRange = 6f;
        [SerializeField, Min(0f)] private float _threatClosingSpeed = 3f;
        [SerializeField, Range(0f, 180f)] private float _threatConeDegrees = 35f;
        [SerializeField, Min(0f)] private float _evadeDistance = 5f;
        [SerializeField, Min(0f)] private float _dropOffStandDepth = 1f;
        [SerializeField, Min(0f)] private float _deliveryMargin = 4f;
        [SerializeField, Min(0)] private int _greedReach = 15;

        [Header("Utility weights")]
        [SerializeField, Min(0f)] private float _collectWeight = 1f;
        [SerializeField, Min(0f)] private float _deliverWeight = 1f;
        [SerializeField, Min(0f)] private float _pileWeight = 1.3f;
        [SerializeField, Min(0f)] private float _ramWeight = 1.2f;
        [SerializeField, Min(0f)] private float _evadeWeight = 1.2f;
        [SerializeField, Min(0f)] private float _commitmentBonus = 0.15f;

        [Header("Utility curves")]
        [SerializeField, Min(0f)] private float _fullBucketBonus = 0.3f;
        [SerializeField, Min(0f)] private float _urgentDeliveryBoost = 2f;
        [SerializeField, Range(0f, 1f)] private float _greedRichnessMinimum = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _greedDiscount = 0.8f;
        [SerializeField, Min(0f)] private float _nearDropOffBonus = 0.3f;
        [SerializeField, Min(0.1f)] private float _nearDropOffTravelTime = 6f;
        [SerializeField, Range(0f, 1f)] private float _collectFloor = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _pileFullShare = 0.9f;
        [SerializeField, Range(0f, 1f)] private float _pileScarcityBase = 0.6f;
        [SerializeField, Min(0f)] private float _ramLoadOffset = 1.3f;
        [SerializeField, Min(0f)] private float _evadeBase = 0.4f;

        [Header("Brain")]
        [SerializeField, Min(0f)] private float _dropOffZoneMargin = 0.3f;
        [SerializeField, Min(0.1f)] private float _arrivalSlowdownDistance = 3f;
        [SerializeField, Range(0f, 1f)] private float _minArrivalThrottle = 0.25f;
        [SerializeField, Min(0f)] private float _goalMovedDistance = 1.5f;
        [SerializeField, Range(0.1f, 1f)] private float _unstuckThrottle = 0.8f;
        [SerializeField, Range(0f, 180f)] private float _unstuckSpreadDegrees = 45f;
        [SerializeField, Min(1)] private int _openDirectionRadius = 2;
        [SerializeField, Min(1f)] private float _detourFactor = 1.3f;
        [SerializeField, Range(0f, 1f)] private float _keptSnowTargetRichness = 0.3f;
        [SerializeField, Min(0f)] private float _snowTargetPatience = 3f;

        public float SnowRefreshInterval => _snowRefreshInterval;

        public float StuckReportTime => _stuckReportTime;

        public BotProfile ProfileFor(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.Strong:
                    return _strong.ToProfile();
                case BotDifficulty.Weak:
                    return _weak.ToProfile();
                default:
                    return _medium.ToProfile();
            }
        }

        public BotSettings ToSettings()
        {
            return new BotSettings
            {
                NavCellSize = _navCellSize,
                NavClearance = _navClearance,
                WaypointReach = _waypointReach,
                ReplanInterval = _replanInterval,
                AvoidRadius = _avoidRadius,
                AvoidStrength = _avoidStrength,
                StuckDistance = _stuckDistance,
                StuckTime = _stuckTime,
                UnstuckDuration = _unstuckDuration,
                CruiseSpeed = _cruiseSpeed,
                SnowTargetReach = _snowTargetReach,
                SnowFalloff = _snowFalloff,
                CrowdRadius = _crowdRadius,
                CrowdPenalty = _crowdPenalty,
                MinPileSteps = _minPileSteps,
                PileFalloff = _pileFalloff,
                RamRange = _ramRange,
                RamMinVictimLoad = _ramMinVictimLoad,
                RamFalloff = _ramFalloff,
                RamLeadTime = _ramLeadTime,
                ThreatRange = _threatRange,
                ThreatClosingSpeed = _threatClosingSpeed,
                ThreatConeDegrees = _threatConeDegrees,
                EvadeDistance = _evadeDistance,
                DropOffStandDepth = _dropOffStandDepth,
                DeliveryMargin = _deliveryMargin,
                GreedReach = _greedReach,
                CollectWeight = _collectWeight,
                DeliverWeight = _deliverWeight,
                PileWeight = _pileWeight,
                RamWeight = _ramWeight,
                EvadeWeight = _evadeWeight,
                CommitmentBonus = _commitmentBonus,
                WeakShare = _weakShare,
                FullBucketBonus = _fullBucketBonus,
                UrgentDeliveryBoost = _urgentDeliveryBoost,
                GreedRichnessMinimum = _greedRichnessMinimum,
                GreedDiscount = _greedDiscount,
                NearDropOffBonus = _nearDropOffBonus,
                NearDropOffTravelTime = _nearDropOffTravelTime,
                CollectFloor = _collectFloor,
                PileFullShare = _pileFullShare,
                PileScarcityBase = _pileScarcityBase,
                RamLoadOffset = _ramLoadOffset,
                EvadeBase = _evadeBase,
                DropOffZoneMargin = _dropOffZoneMargin,
                ArrivalSlowdownDistance = _arrivalSlowdownDistance,
                MinArrivalThrottle = _minArrivalThrottle,
                GoalMovedDistance = _goalMovedDistance,
                UnstuckThrottle = _unstuckThrottle,
                UnstuckSpreadDegrees = _unstuckSpreadDegrees,
                OpenDirectionRadius = _openDirectionRadius,
                DetourFactor = _detourFactor,
                KeptSnowTargetRichness = _keptSnowTargetRichness,
                SnowTargetPatience = _snowTargetPatience,
            };
        }
    }
}
