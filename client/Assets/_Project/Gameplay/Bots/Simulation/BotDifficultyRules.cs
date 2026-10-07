namespace PlowParty.Gameplay.Bots.Simulation
{
    public static class BotDifficultyRules
    {
        public static BotDifficulty Choose(bool strongSeated, double roll, float weakShare)
        {
            if (!strongSeated)
            {
                return BotDifficulty.Strong;
            }

            return roll < weakShare ? BotDifficulty.Weak : BotDifficulty.Medium;
        }
    }
}
