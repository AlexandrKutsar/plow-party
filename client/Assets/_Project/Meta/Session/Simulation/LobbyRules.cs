using System;
using PlowParty.Infrastructure.Network;
using PlowParty.Shared;

namespace PlowParty.Meta.Session.Simulation
{
    public static class LobbyRules
    {
        private const float MillisecondsPerSecond = 1000f;

        public static bool ShouldStart(LobbyMode mode, int players, int maxSlots, float secondsLeft, bool startRequested)
        {
            if (players < 1)
            {
                return false;
            }

            return mode == LobbyMode.Room ? startRequested : players >= maxSlots || secondsLeft <= 0f;
        }

        public static MatchmakingResult MatchmakingResultFor(int players, int maxSlots)
        {
            return new MatchmakingResult(Math.Clamp(players, 1, maxSlots), maxSlots);
        }

        public static bool FoundNothingToJoin(SessionStartOutcome outcome)
        {
            return outcome == SessionStartOutcome.NotFound || outcome == SessionStartOutcome.Refused;
        }

        public static float HostJitterSeconds(double roll, float minSeconds, float maxSeconds)
        {
            var clampedRoll = Math.Clamp(roll, 0d, 1d);
            return minSeconds + (float)clampedRoll * Math.Max(0f, maxSeconds - minSeconds);
        }

        public static float SecondsLeft(long deadlineUnixMilliseconds, long nowUnixMilliseconds)
        {
            return Math.Max(0f, (deadlineUnixMilliseconds - nowUnixMilliseconds) / MillisecondsPerSecond);
        }
    }
}
