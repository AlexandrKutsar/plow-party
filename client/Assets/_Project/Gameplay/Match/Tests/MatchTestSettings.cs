using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Tests
{
    internal static class MatchTestSettings
    {
        public static MatchRules CreateRules(float resultsDuration = 8f)
        {
            return new MatchRules(new MatchSettings
            {
                CountdownDuration = 3f,
                PlayingDuration = 180f,
                ResultsDuration = resultsDuration,
            });
        }
    }
}
