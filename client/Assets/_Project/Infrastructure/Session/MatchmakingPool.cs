namespace PlowParty.Infrastructure.Session
{
    public sealed class MatchmakingPool
    {
        public MatchmakingPool(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
