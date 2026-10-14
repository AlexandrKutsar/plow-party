using System;
using PlowParty.Shared;

namespace PlowParty.Meta.Session.Simulation
{
    public static class LobbyRules
    {
        private const float MillisecondsPerSecond = 1000f;

        public static bool ShouldStart(int players, int maxSlots, float secondsLeft)
        {
            return players >= 1 && (players >= maxSlots || secondsLeft <= 0f);
        }

        public static MatchmakingResult MatchmakingResultFor(int players, int maxSlots, PartyMode mode)
        {
            return new MatchmakingResult(Math.Clamp(players, 1, maxSlots), maxSlots, mode);
        }

        public static long StartsAt(long openedAtUnixMilliseconds, float searchSeconds)
        {
            return openedAtUnixMilliseconds + (long)(searchSeconds * MillisecondsPerSecond);
        }

        public static float SecondsLeft(long deadlineUnixMilliseconds, long nowUnixMilliseconds)
        {
            return Math.Max(0f, (deadlineUnixMilliseconds - nowUnixMilliseconds) / MillisecondsPerSecond);
        }
    }
}
