using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Tests
{
    internal static class MatchTestSettings
    {
        public static MatchSettings Create(float resultsDuration = 8f)
        {
            return new MatchSettings
            {
                WaitingDuration = 15f,
                BotArrivalStart = 1f,
                BotArrivalEnd = 8f,
                FallbackSlotCount = 6,
                CountdownDuration = 3f,
                PlayingDuration = 180f,
                ResultsDuration = resultsDuration,
            };
        }

        public static MatchRules CreateRules(float resultsDuration = 8f)
        {
            return new MatchRules(Create(resultsDuration));
        }

        public static WaitingRules CreateWaitingRules()
        {
            return new WaitingRules(Create());
        }
    }
}
