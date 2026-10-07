using System;
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

        public static MatchLineup LineupFor(int players, int maxSlots)
        {
            return new MatchLineup(Math.Clamp(players, 1, maxSlots), maxSlots);
        }

        public static float SecondsLeft(long deadlineUnixMilliseconds, long nowUnixMilliseconds)
        {
            return Math.Max(0f, (deadlineUnixMilliseconds - nowUnixMilliseconds) / MillisecondsPerSecond);
        }
    }
}
