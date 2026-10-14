using System;
using PlowParty.Meta.Session;
using PlowParty.Shared;

namespace PlowParty.Meta.Lobby.Simulation
{
    public static class LobbyText
    {
        private const string SearchTitleText = "Поиск игры…";
        private const string StopSearchText = "Остановить поиск";
        private const int SecondsPerMinute = 60;
        private const string ConnectingTitle = "Подключение…";
        private const string StartingText = "Матч начинается…";
        private const string ReadyButtonText = "Я готов";
        private const string NotReadyButtonText = "Не готов";
        private const string ReadyMark = "Готов";
        private const string NotReadyMark = "Не готов";
        private const string CustomGameTitleText = "Своя игра";
        private const string NotCountedText = "Не в зачёт турнира";
        private const string SearchButtonText = "Искать матч";
        private const string StartButtonText = "Начать";
        private const string EmptySlotText = "Пусто";
        private static readonly string[] CritterNames = { "Лиса", "Медведь", "Заяц", "Енот", "Пингвин", "Бобр" };

        public static string CustomGameTitle()
        {
            return CustomGameTitleText;
        }

        public static string NotCounted()
        {
            return NotCountedText;
        }

        public static string SearchButton(PartyMode mode)
        {
            return mode == PartyMode.CustomGame ? StartButtonText : SearchButtonText;
        }

        public static string EmptySlot()
        {
            return EmptySlotText;
        }

        public static string BotSlot(BotDifficulty difficulty)
        {
            return $"Бот · {Difficulty(difficulty)}";
        }

        public static string Difficulty(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.Weak:
                    return "слабый";
                case BotDifficulty.Strong:
                    return "сильный";
                default:
                    return "средний";
            }
        }

        public static string CritterName(int critter)
        {
            return critter >= 0 && critter < CritterNames.Length ? CritterNames[critter] : string.Empty;
        }


        public static string SearchTitle()
        {
            return SearchTitleText;
        }

        public static string StopSearch()
        {
            return StopSearchText;
        }

        public static string SearchStatus(LobbyStage stage, float seconds)
        {
            return stage == LobbyStage.Starting ? StartingText : Stopwatch(seconds);
        }

        public static string Stopwatch(float seconds)
        {
            var whole = WholeSeconds(seconds);
            return $"{whole / SecondsPerMinute}:{whole % SecondsPerMinute:00}";
        }

        public static int WholeSeconds(float seconds)
        {
            return Math.Max(0, (int)Math.Floor(seconds));
        }

        public static string PartyTitle(bool connecting, string code)
        {
            return connecting || code == null ? ConnectingTitle : $"Группа {code}";
        }

        public static string ReadyButton(bool isReady)
        {
            return isReady ? NotReadyButtonText : ReadyButtonText;
        }

        public static string MemberReady(bool isReady)
        {
            return isReady ? ReadyMark : NotReadyMark;
        }
    }
}
