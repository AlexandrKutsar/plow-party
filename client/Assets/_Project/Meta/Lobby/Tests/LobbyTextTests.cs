using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Meta.Session;

namespace PlowParty.Meta.Lobby.Tests
{
    public sealed class LobbyTextTests
    {
        [Test]
        public void PartySize_CountAndSlots_ShowsBoth()
        {
            Assert.That(LobbyText.PartySize(3, 6), Is.EqualTo("Игроки: 3 / 6"));
        }

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
