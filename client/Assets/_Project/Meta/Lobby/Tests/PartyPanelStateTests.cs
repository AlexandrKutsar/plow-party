using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Party;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Lobby.Tests
{
    public sealed class PartyPanelStateTests
    {
        private const int Leader = 1;
        private const int Member = 2;
        private const int Other = 3;

        [Test]
        public void For_Connecting_ShowsConnectingWithoutActions()
        {
            var state = PartyPanelState.For(PartyStage.Connecting, "AB3XZ", new PartyState(6), Member, false);

            Assert.That(state.Title, Is.EqualTo("Подключение…"));
            Assert.That(state.CanCopy, Is.False);
            Assert.That(state.ShowReady, Is.False);
            Assert.That(state.ShowSearch, Is.False);
            Assert.That(state.Rows, Is.Empty);
        }

        [Test]
        public void For_InParty_TitleNamesTheCode()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), Member, false);

            Assert.That(state.Title, Is.EqualTo("Группа AB3XZ"));
            Assert.That(state.Code, Is.EqualTo("AB3XZ"));
            Assert.That(state.CanCopy, Is.True);
        }

        [Test]
        public void For_InParty_RowsFollowJoinOrderWithCrownAndReady()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), Member, false);

            Assert.That(state.Rows.Count, Is.EqualTo(3));
            Assert.That(state.Rows[0].Nickname, Is.EqualTo("Аня"));
            Assert.That(state.Rows[0].HasCrown, Is.True);
            Assert.That(state.Rows[0].ReadyText, Is.Empty);
            Assert.That(state.Rows[1].HasCrown, Is.False);
            Assert.That(state.Rows[1].ReadyText, Is.EqualTo("Готов"));
            Assert.That(state.Rows[2].ReadyText, Is.EqualTo("Не готов"));
        }

        [Test]
        public void For_Leader_MayRemoveEveryoneButThemself()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), Leader, true);

            Assert.That(state.Rows[0].CanRemove, Is.False);
            Assert.That(state.Rows[1].CanRemove, Is.True);
            Assert.That(state.Rows[2].CanRemove, Is.True);
        }

        [Test]
        public void For_Member_MayRemoveNobody()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), Member, false);

            Assert.That(state.Rows[0].CanRemove || state.Rows[1].CanRemove || state.Rows[2].CanRemove, Is.False);
        }

        [Test]
        public void For_Leader_SearchesInsteadOfReady()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), Leader, true);

            Assert.That(state.ShowSearch, Is.True);
            Assert.That(state.CanSearch, Is.True);
            Assert.That(state.ShowReady, Is.False);
        }

        [Test]
        public void For_LeaderWhileSearchIsNotAllowed_SearchIsDisabled()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), Leader, false);

            Assert.That(state.CanSearch, Is.False);
        }

        [TestCase(Member, "Не готов")]
        [TestCase(Other, "Я готов")]
        public void For_Member_ReadyButtonTogglesOwnReady(int localId, string expected)
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), localId, false);

            Assert.That(state.ShowSearch, Is.False);
            Assert.That(state.ShowReady, Is.True);
            Assert.That(state.ReadyLabel, Is.EqualTo(expected));
            Assert.That(state.IsReady, Is.EqualTo(localId == Member));
        }

        [Test]
        public void For_AnyMember_SeesTheMode()
        {
            var party = Party();
            party.SetMode(Leader, PartyMode.CustomGame);

            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", party, Member, false);

            Assert.That(state.Mode, Is.EqualTo(PartyMode.CustomGame));
        }

        [TestCase(Leader, true)]
        [TestCase(Member, false)]
        public void For_OnlyTheLeader_PicksQuickPlay(int localId, bool expected)
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), localId, true);

            Assert.That(state.CanPickQuickPlay, Is.EqualTo(expected));
        }

        [Test]
        public void For_Leader_CustomGameIsNotAvailableYet()
        {
            var state = PartyPanelState.For(PartyStage.InParty, "AB3XZ", Party(), Leader, true);

            Assert.That(state.CanPickCustomGame, Is.False);
        }

        private static PartyState Party()
        {
            var party = new PartyState(6);
            party.Join(Leader, "Аня");
            party.Join(Member, "Борис");
            party.Join(Other, "Вера");
            party.SetReady(Member, true);
            return party;
        }
    }
}
