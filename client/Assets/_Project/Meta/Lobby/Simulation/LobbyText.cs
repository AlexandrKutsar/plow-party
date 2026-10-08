using System;
using System.Text;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Meta.Session;

namespace PlowParty.Meta.Lobby.Simulation
{
    public static class LobbyText
    {
        private const string QuickPlayTitle = "Быстрая игра";
        private const string ConnectingTitle = "Подключение…";
        private const string StartingText = "Матч начинается…";
        private const string SearchText = "Искать матч";
        private const string ReadyText = "Я готов";
        private const string NotReadyText = "Не готов";
        private const string LeaderMark = " (лидер)";
        private const string ReadyMark = " — готов";
        private const string NotReadyMark = " — не готов";
        private const string MemberSeparator = ", ";

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

        public static string Title(LobbyStage stage)
        {
            return stage == LobbyStage.Connecting ? ConnectingTitle : QuickPlayTitle;
        }

        public static string Status(LobbyStage stage, float secondsLeft)
        {
            switch (stage)
            {
                case LobbyStage.Starting:
                    return StartingText;
                case LobbyStage.Gathering:
                    return SearchTimer(secondsLeft);
                default:
                    return string.Empty;
            }
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

        public static string PartyStarting()
        {
            return StartingText;
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
