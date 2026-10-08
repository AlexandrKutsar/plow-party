using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Tests
{
    internal static class MatchTestSettings
    {
        public static MatchSettings Create()
        {
            return new MatchSettings
            {
                WaitingDuration = 15f,
                BotArrivalStart = 1f,
                BotArrivalEnd = 8f,
                QuickBotArrivalStart = 0.5f,
                QuickBotArrivalEnd = 2.5f,
                FallbackSlotCount = 6,
                CountdownDuration = 3f,
                PlayingDuration = 180f,
            };
        }

        public static MatchRules CreateRules()
        {
            return new MatchRules(Create());
        }

        public static WaitingRules CreateWaitingRules()
        {
            return new WaitingRules(Create());
        }
    }
}
