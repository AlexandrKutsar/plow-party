using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Session;

namespace PlowParty.Meta.Lobby.Tests
{
    public sealed class LobbyTextTests
    {
        [TestCase(0f, "0:00")]
        [TestCase(7.9f, "0:07")]
        [TestCase(65f, "1:05")]
        public void Stopwatch_SecondsSinceThePress_ShowsMinutesAndSeconds(float seconds, string expected)
        {
            Assert.That(LobbyText.Stopwatch(seconds), Is.EqualTo(expected));
        }

        [Test]
        public void SearchStatus_Searching_ShowsTheStopwatch()
        {
            Assert.That(LobbyText.SearchStatus(LobbyStage.Searching, 12f), Is.EqualTo("0:12"));
        }

        [Test]
        public void SearchStatus_Starting_IsStarting()
        {
            Assert.That(LobbyText.SearchStatus(LobbyStage.Starting, 12f), Is.EqualTo("Матч начинается…"));
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

        [TestCase(false, "Я готов")]
        [TestCase(true, "Не готов")]
        public void ReadyButton_OwnReady_NamesTheToggle(bool isReady, string expected)
        {
            Assert.That(LobbyText.ReadyButton(isReady), Is.EqualTo(expected));
        }

        [TestCase(true, "Готов")]
        [TestCase(false, "Не готов")]
        public void MemberReady_MemberReady_NamesTheState(bool isReady, string expected)
        {
            Assert.That(LobbyText.MemberReady(isReady), Is.EqualTo(expected));
        }

        [Test]
        public void StopSearch_Always_NamesTheStop()
        {
            Assert.That(LobbyText.StopSearch(), Is.EqualTo("Остановить поиск"));
        }
    }
}
