using System;
using PlowParty.Meta.Session;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Lobby.Simulation
{
    public static class LobbyText
    {
        private const string QuickPlayTitle = "Быстрая игра";
        private const string ConnectingTitle = "Подключение…";
        private const string WaitingForHost = "Ждём, пока хост начнёт матч";
        private const string ShareCode = "Сообщите код друзьям и нажмите «Начать»";
        private const string StartingText = "Матч начинается…";

        public static string Players(int count, int maxSlots)
        {
            return $"Игроки: {count} / {maxSlots}";
        }

        public static string SearchTimer(float secondsLeft)
        {
            return $"Поиск игроков: {WholeSeconds(secondsLeft)} с";
        }

        public static int WholeSeconds(float secondsLeft)
        {
            return Math.Max(0, (int)Math.Ceiling(secondsLeft));
        }

        public static string Title(LobbyStage stage, LobbyMode mode, string roomCode)
        {
            if (stage == LobbyStage.Connecting)
            {
                return ConnectingTitle;
            }

            return mode == LobbyMode.Room ? $"Комната {roomCode}" : QuickPlayTitle;
        }

        public static string Status(LobbyStage stage, LobbyMode mode, bool isHost, float secondsLeft)
        {
            switch (stage)
            {
                case LobbyStage.Starting:
                    return StartingText;
                case LobbyStage.Gathering when mode == LobbyMode.Room:
                    return isHost ? ShareCode : WaitingForHost;
                case LobbyStage.Gathering:
                    return SearchTimer(secondsLeft);
                default:
                    return string.Empty;
            }
        }
    }
}
