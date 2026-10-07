namespace PlowParty.Meta.Tournament.Simulation
{
    public static class TournamentText
    {
        public const string Loading = "Загрузка…";
        public const string Offline = "Нет связи";

        private const string NotRanked = "Сыграйте матч, чтобы попасть в таблицу";
        private const string GapRank = "…";

        public static string Summary(TournamentBoard board)
        {
            if (board == null)
            {
                return Offline;
            }

            return board.MyRank > 0 ? $"Ваше место: {board.MyRank} из {board.Players}" : NotRanked;
        }

        public static string MedalLabel(Medal medal)
        {
            switch (medal)
            {
                case Medal.Gold:
                    return "Медаль: золото";
                case Medal.Silver:
                    return "Медаль: серебро";
                case Medal.Bronze:
                    return "Медаль: бронза";
                default:
                    return string.Empty;
            }
        }

        public static string Rank(Standing standing)
        {
            return standing.IsGap ? GapRank : $"{standing.Rank}.";
        }
    }
}
