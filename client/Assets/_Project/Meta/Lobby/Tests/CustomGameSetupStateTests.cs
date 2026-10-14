using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Party;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Meta.Session;
using PlowParty.Shared;

namespace PlowParty.Meta.Lobby.Tests
{
    public sealed class CustomGameSetupStateTests
    {
        private const int Leader = 1;
        private const int Member = 2;

        [Test]
        public void For_LeaderInCustomGame_ShowsSixSlotsWithBotControls()
        {
            var party = CustomParty();
            party.SetBot(Leader, 3, BotDifficulty.Strong);

            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", party, Leader, true);

            Assert.That(state.ShowSetup, Is.True);
            Assert.That(state.Slots.Count, Is.EqualTo(6));
            Assert.That(state.Slots[0].Label, Is.EqualTo("Аня"));
            Assert.That(state.Slots[2].Label, Is.EqualTo("Пусто"));
            Assert.That(state.Slots[2].CanAddBot, Is.True);
            Assert.That(state.Slots[3].Label, Is.EqualTo("Бот · сильный"));
            Assert.That(state.Slots[3].CanChangeBot, Is.True);
        }

        [Test]
        public void For_MemberInCustomGame_CannotTouchBots()
        {
            var party = CustomParty();
            party.SetBot(Leader, 3, BotDifficulty.Weak);

            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", party, Member, false);

            Assert.That(state.Slots[2].CanAddBot, Is.False);
            Assert.That(state.Slots[3].CanChangeBot, Is.False);
        }

        [Test]
        public void For_CustomGame_MarksItNotCountedAndStartsInsteadOfSearching()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", CustomParty(), Leader, true);

            Assert.That(state.NotCountedText, Is.EqualTo("Не в зачёт турнира"));
            Assert.That(state.SearchLabel, Is.EqualTo("Начать"));
        }

        [Test]
        public void For_QuickPlay_HidesTheSetup()
        {
            var party = CustomParty();
            party.SetMode(Leader, PartyMode.QuickPlay);

            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", party, Leader, true);

            Assert.That(state.ShowSetup, Is.False);
            Assert.That(state.SearchLabel, Is.EqualTo("Искать матч"));
        }

        [Test]
        public void For_ColorOfAnotherMember_IsUnavailable()
        {
            var party = CustomParty();
            party.PickColor(Member, 4);
            party.PickColor(Leader, 6);

            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", party, Leader, true);

            Assert.That(state.Colors[4].IsTaken, Is.True);
            Assert.That(state.Colors[6].IsMine, Is.True);
            Assert.That(state.Colors[6].IsTaken, Is.False);
            Assert.That(state.Colors[0].IsTaken, Is.False);
        }

        [Test]
        public void For_LocalCritter_IsNamed()
        {
            var party = CustomParty();
            party.PickCritter(Leader, 1);

            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", party, Leader, true);

            Assert.That(state.Critter, Is.EqualTo(1));
            Assert.That(state.CritterName, Is.EqualTo("Медведь"));
        }

        [Test]
        public void For_SoloCustomGame_StartsAloneWithoutPartyActions()
        {
            var solo = new PartyState(6, PartyMode.CustomGame);
            solo.Join(PartyState.SoloId, "Аня");

            var state = PartyPanelState.For(PartyStage.None, null, solo, PartyState.SoloId, true);

            Assert.That(state.Title, Is.EqualTo("Своя игра"));
            Assert.That(state.CanCopy, Is.False);
            Assert.That(state.ShowLeave, Is.False);
            Assert.That(state.ShowSearch, Is.True);
            Assert.That(state.CanSearch, Is.True);
            Assert.That(state.CanPickQuickPlay, Is.True);
            Assert.That(state.Slots[1].CanAddBot, Is.True);
        }

        [Test]
        public void LobbyScreens_SoloInCustomGame_ShowsTheSetupScreen()
        {
            Assert.That(LobbyScreens.For(PartyStage.None, LobbyStage.Idle, PartyMode.CustomGame), Is.EqualTo(LobbyScreen.Party));
            Assert.That(LobbyScreens.For(PartyStage.None, LobbyStage.Idle, PartyMode.QuickPlay), Is.EqualTo(LobbyScreen.Solo));
        }

        [TestCase(BotDifficulty.Weak, BotDifficulty.Medium)]
        [TestCase(BotDifficulty.Medium, BotDifficulty.Strong)]
        [TestCase(BotDifficulty.Strong, BotDifficulty.Weak)]
        public void NextDifficulty_Cycles(BotDifficulty current, BotDifficulty expected)
        {
            Assert.That(PartyPanelState.NextDifficulty(current), Is.EqualTo(expected));
        }

        private static PartyState CustomParty()
        {
            var party = new PartyState(6, PartyMode.CustomGame);
            party.Join(Leader, "Аня");
            party.Join(Member, "Борис");
            return party;
        }
    }
}
