using NUnit.Framework;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Party.Tests
{
    public sealed class PartyStateTests
    {
        private const int Capacity = 6;
        private const int Anna = 1;
        private const int Boris = 2;
        private const int Vera = 3;

        [Test]
        public void Join_FirstMember_BecomesLeader()
        {
            var party = new PartyState(Capacity);

            party.Join(Anna, "Anna");

            Assert.That(party.LeaderId, Is.EqualTo(Anna));
        }

        [Test]
        public void Join_LaterMembers_KeepJoinOrderAndLeader()
        {
            var party = PartyOf(Anna, Boris, Vera);

            Assert.That(party.LeaderId, Is.EqualTo(Anna));
            Assert.That(party.Members[1].Id, Is.EqualTo(Boris));
            Assert.That(party.Members[2].Id, Is.EqualTo(Vera));
        }

        [Test]
        public void Join_NewMember_IsNotReady()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.Members[1].IsReady, Is.False);
        }

        [Test]
        public void Join_PartyOfSix_RefusesSeventhAsFull()
        {
            var party = PartyOf(1, 2, 3, 4, 5, 6);

            Assert.That(party.Join(7, "Late"), Is.EqualTo(PartyJoinOutcome.Full));
            Assert.That(party.Count, Is.EqualTo(Capacity));
        }

        [Test]
        public void Join_SameMemberTwice_StaysOneMember()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.Join(Boris, "Boris"), Is.EqualTo(PartyJoinOutcome.AlreadyMember));
            Assert.That(party.Count, Is.EqualTo(2));
        }

        [Test]
        public void Leave_Leader_PassesLeadershipToLongestStandingMember()
        {
            var party = PartyOf(Anna, Boris, Vera);

            party.Leave(Anna);

            Assert.That(party.LeaderId, Is.EqualTo(Boris));
        }

        [Test]
        public void Leave_LastMember_LeavesNoLeader()
        {
            var party = PartyOf(Anna);

            party.Leave(Anna);

            Assert.That(party.HasLeader, Is.False);
        }

        [Test]
        public void Remove_ByLeader_DropsMember()
        {
            var party = PartyOf(Anna, Boris, Vera);

            Assert.That(party.Remove(Anna, Boris), Is.True);
            Assert.That(party.Contains(Boris), Is.False);
        }

        [Test]
        public void Remove_ByNonLeader_IsRefused()
        {
            var party = PartyOf(Anna, Boris, Vera);

            Assert.That(party.Remove(Boris, Vera), Is.False);
            Assert.That(party.Contains(Vera), Is.True);
        }

        [Test]
        public void Remove_LeaderThemself_IsRefused()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.Remove(Anna, Anna), Is.False);
            Assert.That(party.LeaderId, Is.EqualTo(Anna));
        }

        [Test]
        public void SetReady_Member_ChangesOnlyTheirReady()
        {
            var party = PartyOf(Anna, Boris, Vera);

            party.SetReady(Boris, true);

            Assert.That(party.Members[1].IsReady, Is.True);
            Assert.That(party.Members[2].IsReady, Is.False);
        }

        [Test]
        public void SetReady_Leader_IsRefused()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.SetReady(Anna, true), Is.False);
        }

        [Test]
        public void CanStart_SoloLeader_IsTrue()
        {
            Assert.That(PartyOf(Anna).CanStart, Is.True);
        }

        [Test]
        public void CanStart_SomeMemberNotReady_IsFalse()
        {
            var party = PartyOf(Anna, Boris, Vera);
            party.SetReady(Boris, true);

            Assert.That(party.CanStart, Is.False);
        }

        [Test]
        public void StartSearch_LeaderWithEveryoneReady_Searches()
        {
            var party = PartyOf(Anna, Boris);
            party.SetReady(Boris, true);

            Assert.That(party.StartSearch(Anna), Is.True);
            Assert.That(party.IsSearching, Is.True);
        }

        [Test]
        public void StartSearch_MemberNotReady_DoesNotSearch()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.StartSearch(Anna), Is.False);
            Assert.That(party.IsSearching, Is.False);
        }

        [Test]
        public void StartSearch_ByMember_DoesNotSearch()
        {
            var party = PartyOf(Anna, Boris);
            party.SetReady(Boris, true);

            Assert.That(party.StartSearch(Boris), Is.False);
            Assert.That(party.IsSearching, Is.False);
        }

        [Test]
        public void StopSearch_ByAnyMember_EndsTheSearch()
        {
            var party = PartyOf(Anna, Boris);
            party.SetReady(Boris, true);
            party.StartSearch(Anna);

            party.StopSearch(Boris);

            Assert.That(party.IsSearching, Is.False);
        }

        [Test]
        public void Join_StrangerWhileSearching_IsAGuestNotAMember()
        {
            var party = PartyOf(Anna);
            party.StartSearch(Anna);

            Assert.That(party.Join(Boris, "Boris"), Is.EqualTo(PartyJoinOutcome.Guest));
            Assert.That(party.Contains(Boris), Is.False);
        }

        [Test]
        public void CanStart_EveryOtherMemberReady_IsTrue()
        {
            var party = PartyOf(Anna, Boris, Vera);
            party.SetReady(Boris, true);
            party.SetReady(Vera, true);

            Assert.That(party.CanStart, Is.True);
        }

        [Test]
        public void StopSearch_ByMember_MakesOnlyThatMemberNotReady()
        {
            var party = PartyOf(Anna, Boris, Vera);
            party.SetReady(Boris, true);
            party.SetReady(Vera, true);

            party.StopSearch(Vera);

            Assert.That(party.Members[1].IsReady, Is.True);
            Assert.That(party.Members[2].IsReady, Is.False);
            Assert.That(party.CanStart, Is.False);
        }

        [Test]
        public void Leave_LeaderWithReadySuccessor_SuccessorLeadsAndPartyCanStart()
        {
            var party = PartyOf(Anna, Boris, Vera);
            party.SetReady(Boris, true);
            party.SetReady(Vera, true);

            party.Leave(Anna);

            Assert.That(party.LeaderId, Is.EqualTo(Boris));
            Assert.That(party.CanStart, Is.True);
        }

        [Test]
        public void SetMode_ByLeader_ChangesMode()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.SetMode(Anna, PartyMode.CustomGame), Is.True);
            Assert.That(party.Mode, Is.EqualTo(PartyMode.CustomGame));
        }

        [Test]
        public void SetMode_ByMember_IsRefused()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.SetMode(Boris, PartyMode.CustomGame), Is.False);
            Assert.That(party.Mode, Is.EqualTo(PartyMode.QuickPlay));
        }

        [Test]
        public void Restore_Members_KeepsOrderReadyAndMode()
        {
            var party = PartyState.Restore(Capacity, PartyMode.CustomGame, new[]
            {
                new PartyMember(Boris, "Boris", false),
                new PartyMember(Anna, "Anna", true),
            });

            Assert.That(party.LeaderId, Is.EqualTo(Boris));
            Assert.That(party.Members[1].IsReady, Is.True);
            Assert.That(party.Mode, Is.EqualTo(PartyMode.CustomGame));
        }

        [Test]
        public void Restore_SearchingParty_KeepsSearching()
        {
            var party = PartyState.Restore(Capacity, PartyMode.QuickPlay, new[] { new PartyMember(Anna, "Anna", false) }, true);

            Assert.That(party.IsSearching, Is.True);
        }

        private static PartyState PartyOf(params int[] ids)
        {
            var party = new PartyState(Capacity);
            foreach (var id in ids)
            {
                party.Join(id, $"Member {id}");
            }

            return party;
        }
    }
}
