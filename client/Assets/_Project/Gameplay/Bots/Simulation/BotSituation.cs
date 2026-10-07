namespace PlowParty.Gameplay.Bots.Simulation
{
    public struct BotSituation
    {
        public int Load { get; set; }

        public int Capacity { get; set; }

        public int NextTierLoad { get; set; }

        public float SnowRichness { get; set; }

        public float DropOffTravelTime { get; set; }

        public float PlayingRemaining { get; set; }

        public bool HasPile { get; set; }

        public float PileDistance { get; set; }

        public bool HasRamTarget { get; set; }

        public int RamTargetLoad { get; set; }

        public float RamTargetDistance { get; set; }

        public float ThreatLevel { get; set; }

        public float LoadShare => Capacity > 0 ? (float)Load / Capacity : 0f;
    }
}
