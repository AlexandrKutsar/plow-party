namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotSettings
    {
        public float NavCellSize { get; set; }

        public float NavClearance { get; set; }

        public float WaypointReach { get; set; }

        public float ReplanInterval { get; set; }

        public float AvoidRadius { get; set; }

        public float AvoidStrength { get; set; }

        public float StuckDistance { get; set; }

        public float StuckTime { get; set; }

        public float UnstuckDuration { get; set; }

        public float CruiseSpeed { get; set; }

        public float SnowTargetReach { get; set; }

        public float SnowFalloff { get; set; }

        public float CrowdRadius { get; set; }

        public float CrowdPenalty { get; set; }

        public int MinPileSteps { get; set; }

        public float PileFalloff { get; set; }

        public float RamRange { get; set; }

        public int RamMinVictimLoad { get; set; }

        public float RamFalloff { get; set; }

        public float RamLeadTime { get; set; }

        public float ThreatRange { get; set; }

        public float ThreatClosingSpeed { get; set; }

        public float ThreatConeDegrees { get; set; }

        public float EvadeDistance { get; set; }

        public float DropOffStandDepth { get; set; }

        public float DeliveryMargin { get; set; }

        public int GreedReach { get; set; }

        public float CollectWeight { get; set; }

        public float DeliverWeight { get; set; }

        public float PileWeight { get; set; }

        public float RamWeight { get; set; }

        public float EvadeWeight { get; set; }

        public float CommitmentBonus { get; set; }

        public float WeakShare { get; set; }

        public float FullBucketBonus { get; set; }

        public float UrgentDeliveryBoost { get; set; }

        public float GreedRichnessMinimum { get; set; }

        public float GreedDiscount { get; set; }

        public float NearDropOffBonus { get; set; }

        public float NearDropOffTravelTime { get; set; }

        public float CollectFloor { get; set; }

        public float PileFullShare { get; set; }

        public float PileScarcityBase { get; set; }

        public float RamLoadOffset { get; set; }

        public float EvadeBase { get; set; }

        public float DropOffZoneMargin { get; set; }

        public float ArrivalSlowdownDistance { get; set; }

        public float MinArrivalThrottle { get; set; }

        public float GoalMovedDistance { get; set; }

        public float UnstuckThrottle { get; set; }

        public float UnstuckSpreadDegrees { get; set; }

        public int OpenDirectionRadius { get; set; }

        public float DetourFactor { get; set; }

        public float KeptSnowTargetRichness { get; set; }

        public float SnowTargetPatience { get; set; }
    }
}
