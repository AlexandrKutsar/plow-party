using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Session;
using PlowParty.Meta.Session.Simulation;

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
        public void Title_QuickPlay_IsQuickPlay()
        {
            Assert.That(LobbyText.Title(LobbyStage.Gathering, LobbyMode.QuickPlay, null), Is.EqualTo("Быстрая игра"));
        }

        [Test]
        public void Title_Room_ShowsCode()
        {
            Assert.That(LobbyText.Title(LobbyStage.Gathering, LobbyMode.Room, "AB3XZ"), Is.EqualTo("Комната AB3XZ"));
        }

        [Test]
        public void Title_Connecting_IsConnecting()
        {
            Assert.That(LobbyText.Title(LobbyStage.Connecting, LobbyMode.QuickPlay, null), Is.EqualTo("Подключение…"));
        }

        [Test]
        public void Status_RoomGuest_WaitsForHost()
        {
            Assert.That(LobbyText.Status(LobbyStage.Gathering, LobbyMode.Room, false, 5f), Is.EqualTo("Ждём, пока хост начнёт матч"));
        }

        [Test]
        public void Status_RoomHost_AsksToStart()
        {
            Assert.That(LobbyText.Status(LobbyStage.Gathering, LobbyMode.Room, true, 5f), Is.EqualTo("Сообщите код друзьям и нажмите «Начать»"));
        }

        [Test]
        public void Status_QuickPlay_ShowsTimer()
        {
            Assert.That(LobbyText.Status(LobbyStage.Gathering, LobbyMode.QuickPlay, false, 4.5f), Is.EqualTo("Поиск игроков: 5 с"));
        }

        [Test]
        public void Status_Starting_IsStarting()
        {
            Assert.That(LobbyText.Status(LobbyStage.Starting, LobbyMode.QuickPlay, true, 0f), Is.EqualTo("Матч начинается…"));
        }
    }
}
