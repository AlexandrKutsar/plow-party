using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Party;
using PlowParty.Meta.Session;

namespace PlowParty.Meta.Lobby.Tests
{
    public sealed class LobbyScreensTests
    {
        [Test]
        public void For_NoPartyAndIdle_IsSolo()
        {
            Assert.That(LobbyScreens.For(PartyStage.None, LobbyStage.Idle), Is.EqualTo(LobbyScreen.Solo));
        }

        [TestCase(PartyStage.Connecting)]
        [TestCase(PartyStage.InParty)]
        [TestCase(PartyStage.Away)]
        public void For_PartyAndIdle_IsParty(PartyStage party)
        {
            Assert.That(LobbyScreens.For(party, LobbyStage.Idle), Is.EqualTo(LobbyScreen.Party));
        }

        [TestCase(PartyStage.None, LobbyStage.Searching)]
        [TestCase(PartyStage.InParty, LobbyStage.Searching)]
        [TestCase(PartyStage.Away, LobbyStage.Starting)]
        public void For_SearchRunning_IsSearch(PartyStage party, LobbyStage search)
        {
            Assert.That(LobbyScreens.For(party, search), Is.EqualTo(LobbyScreen.Search));
        }
    }
}
