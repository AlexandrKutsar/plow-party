namespace PlowParty.Gameplay.Match.Simulation
{
    public sealed class MatchSettings
    {
        public float WaitingDuration { get; set; }

        public float BotArrivalStart { get; set; }

        public float BotArrivalEnd { get; set; }

        public int FallbackSlotCount { get; set; }

        public float CountdownDuration { get; set; }

        public float PlayingDuration { get; set; }

        public float ResultsDuration { get; set; }
    }
}
