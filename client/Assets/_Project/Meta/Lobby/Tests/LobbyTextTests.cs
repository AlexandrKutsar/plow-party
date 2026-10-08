using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Meta.Session;

namespace PlowParty.Meta.Lobby.Tests
{
    public sealed class LobbyTextTests
    {
        [Test]
        public void Players_CountAndSlots_ShowsBoth()
        {
            Assert.That(LobbyText.Players(3, 6), Is.EqualTo("Игроки: 3 / 6"));
        }

        [TestCase(9.2f, "Поиск игроков: 10 с")]
        [TestCase(0.4f, "Поиск игроков: 1 с")]
        [TestCase(0f, "Поиск игроков: 0 с")]
        public void SearchTimer_SecondsLeft_RoundsUp(float seconds, string expected)
        {
            Assert.That(LobbyText.SearchTimer(seconds), Is.EqualTo(expected));
        }

        [Test]
        public void Title_Gathering_IsQuickPlay()
        {
            Assert.That(LobbyText.Title(LobbyStage.Gathering), Is.EqualTo("Быстрая игра"));
        }

        [Test]
        public void Title_Connecting_IsConnecting()
        {
            Assert.That(LobbyText.Title(LobbyStage.Connecting), Is.EqualTo("Подключение…"));
        }

        [Test]
        public void Status_Gathering_ShowsTimer()
        {
            Assert.That(LobbyText.Status(LobbyStage.Gathering, 4.5f), Is.EqualTo("Поиск игроков: 5 с"));
        }

        [Test]
        public void Status_Starting_IsStarting()
        {
            Assert.That(LobbyText.Status(LobbyStage.Starting, 0f), Is.EqualTo("Матч начинается…"));
        }

        [Test]
        public void PartyTitle_InParty_ShowsPartyCode()
        {
            Assert.That(LobbyText.PartyTitle(false, "AB3XZ"), Is.EqualTo("Группа AB3XZ"));
        }

        [Test]
        public void PartyTitle_Connecting_IsConnecting()
        {
            Assert.That(LobbyText.PartyTitle(true, "AB3XZ"), Is.EqualTo("Подключение…"));
        }

        [Test]
        public void PartyMembers_LeaderAndMembers_MarksLeaderAndReady()
        {
            var party = new PartyState(6);
            party.Join(1, "Аня");
            party.Join(2, "Борис");
            party.Join(3, "Вера");
            party.SetReady(2, true);

            Assert.That(LobbyText.PartyMembers(party), Is.EqualTo("Аня (лидер), Борис — готов, Вера — не готов"));
        }

        [TestCase(true, false, "Искать матч")]
        [TestCase(false, false, "Я готов")]
        [TestCase(false, true, "Не готов")]
        public void PartyAction_RoleAndReady_NamesTheButton(bool isLeader, bool isReady, string expected)
        {
            Assert.That(LobbyText.PartyAction(isLeader, isReady), Is.EqualTo(expected));
        }
    }
}
