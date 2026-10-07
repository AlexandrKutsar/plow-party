namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotProfile
    {
        public BotDifficulty Difficulty { get; set; }

        public float ReactionDelay { get; set; }

        public float DecisionIntervalMin { get; set; }

        public float DecisionIntervalMax { get; set; }

        public float SteeringNoiseDegrees { get; set; }

        public float MistakeChance { get; set; }

        public float Aggression { get; set; }

        public float Greed { get; set; }

        public float Caution { get; set; }

        public float ThrottleCap { get; set; }
    }
}
