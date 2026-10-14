namespace PlowParty.Meta.Session.Simulation
{
    public static class SessionLossNotice
    {
        public static SessionNotice For(MatchStanding standing)
        {
            switch (standing)
            {
                case MatchStanding.Playing:
                    return SessionNotice.HostLeft;
                case MatchStanding.TurnedAway:
                    return SessionNotice.MatchStarted;
                case MatchStanding.Finished:
                    return SessionNotice.None;
                default:
                    return SessionNotice.ConnectionLost;
            }
        }

        public static bool KeepsResults(MatchStanding standing)
        {
            return standing == MatchStanding.Finished;
        }
    }
}
