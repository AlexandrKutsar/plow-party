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
    }
}
