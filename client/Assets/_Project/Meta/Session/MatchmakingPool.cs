namespace PlowParty.Meta.Session
{
    public sealed class MatchmakingPool
    {
        public MatchmakingPool(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string RoomSessionName(string roomCode)
        {
            return $"{Name}-room-{roomCode}";
        }
    }
}
