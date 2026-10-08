namespace PlowParty.Meta.Session.Simulation
{
    public static class SessionLossNotice
    {
        public const string HostLeftText = "Хост вышел, матч прерван";

        public static string For(MatchStanding standing)
        {
            switch (standing)
            {
                case MatchStanding.Underway:
                    return HostLeftText;
                case MatchStanding.Finished:
                    return null;
                default:
                    return Matchmaker.SessionLostText;
            }
        }
    }
}
