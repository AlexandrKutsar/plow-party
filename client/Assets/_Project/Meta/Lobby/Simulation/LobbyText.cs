using System;
using System.Text;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Meta.Session;

namespace PlowParty.Meta.Lobby.Simulation
{
    public static class LobbyText
    {
        private const string SearchTitleText = "Поиск игры…";
        private const string StopSearchText = "Остановить";
        private const int SecondsPerMinute = 60;
        private const string ConnectingTitle = "Подключение…";
        private const string StartingText = "Матч начинается…";
        private const string SearchText = "Искать матч";
        private const string ReadyText = "Я готов";
        private const string NotReadyText = "Не готов";
        private const string LeaderMark = " (лидер)";
        private const string ReadyMark = " — готов";
        private const string NotReadyMark = " — не готов";
        private const string MemberSeparator = ", ";

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

        public static string PartyMembers(PartyState party)
        {
            var text = new StringBuilder();
            for (var i = 0; i < party.Count; i++)
            {
                var member = party.Members[i];
                if (i > 0)
                {
                    text.Append(MemberSeparator);
                }

                text.Append(member.Nickname);
                text.Append(i == 0 ? LeaderMark : member.IsReady ? ReadyMark : NotReadyMark);
            }

            return text.ToString();
        }

        public static string PartySize(int count, int maxSlots)
        {
            return $"Игроки: {count} / {maxSlots}";
        }

        public static string PartyAction(bool isLeader, bool isReady)
        {
            if (isLeader)
            {
                return SearchText;
            }

            return isReady ? NotReadyText : ReadyText;
        }
    }
}
