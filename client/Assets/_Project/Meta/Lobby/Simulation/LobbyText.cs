using System;
using PlowParty.Meta.Session;

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
