namespace PlowParty.Shared
{
    public readonly struct CustomGameSlot
    {
        private CustomGameSlot(bool isBot, int playerId, int color, int critter, BotDifficulty difficulty)
        {
            IsBot = isBot;
            PlayerId = playerId;
            Color = color;
            Critter = critter;
            Difficulty = difficulty;
        }

        public bool IsBot { get; }

        public int PlayerId { get; }

        public int Color { get; }

        public int Critter { get; }

        public BotDifficulty Difficulty { get; }

        public static CustomGameSlot Player(int playerId, int color, int critter)
        {
            return new CustomGameSlot(false, playerId, color, critter, default);
        }

        public static CustomGameSlot Bot(BotDifficulty difficulty)
        {
            return new CustomGameSlot(true, -1, -1, -1, difficulty);
        }
    }
}
