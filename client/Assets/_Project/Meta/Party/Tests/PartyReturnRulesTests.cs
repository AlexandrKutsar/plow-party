using NUnit.Framework;
using PlowParty.Infrastructure.Network;
using PlowParty.Meta.Party.Simulation;

namespace PlowParty.Meta.Party.Tests
{
    public sealed class PartyReturnRulesTests
    {
        private const float RejoinSeconds = 10f;
        private const float GiveUpSeconds = 20f;
        private const int Anna = 1;
        private const int Boris = 2;
        private const int Vera = 3;

        [Test]
        public void First_Leader_RecreatesTheParty()
        {
            Assert.That(PartyReturnRules.First(PartyRole.Leader), Is.EqualTo(PartyReturnStep.Host));
        }

        [Test]
        public void First_Member_Rejoins()
        {
            Assert.That(PartyReturnRules.First(PartyRole.Member), Is.EqualTo(PartyReturnStep.Join));
        }

        [Test]
        public void Next_LeaderFindsCodeTaken_JoinsTheMemberWhoCameBackFirst()
        {
            Assert.That(Next(PartyRole.Leader, PartyReturnStep.Host, SessionStartOutcome.NameTaken, 1f), Is.EqualTo(PartyReturnStep.Join));
        }

        [Test]
        public void Next_MemberFindsNoPartyWithinRejoinTime_TriesToRejoinAgain()
        {
            Assert.That(Next(PartyRole.Member, PartyReturnStep.Join, SessionStartOutcome.NotFound, 4f), Is.EqualTo(PartyReturnStep.Join));
        }

        [Test]
        public void Next_MemberFindsNoPartyAfterRejoinTime_RecreatesThePartyAndLeads()
        {
            Assert.That(Next(PartyRole.Member, PartyReturnStep.Join, SessionStartOutcome.NotFound, 10.5f), Is.EqualTo(PartyReturnStep.Host));
        }

        [Test]
        public void Next_LeaderJoinFindsNoParty_RecreatesItAgain()
        {
            Assert.That(Next(PartyRole.Leader, PartyReturnStep.Join, SessionStartOutcome.NotFound, 2f), Is.EqualTo(PartyReturnStep.Host));
        }

        [Test]
        public void Next_PartyFull_Stops()
        {
            Assert.That(Next(PartyRole.Member, PartyReturnStep.Join, SessionStartOutcome.Full, 1f), Is.EqualTo(PartyReturnStep.Stop));
        }

        [Test]
        public void Next_AfterGiveUpTime_Stops()
        {
            Assert.That(Next(PartyRole.Member, PartyReturnStep.Host, SessionStartOutcome.NameTaken, 21f), Is.EqualTo(PartyReturnStep.Stop));
        }

        [Test]
        public void Next_PartyStillInItsMatch_KeepsWaitingToRejoin()
        {
            Assert.That(Next(PartyRole.Member, PartyReturnStep.Join, SessionStartOutcome.Refused, 12f), Is.EqualTo(PartyReturnStep.Join));
        }

        [Test]
        public void Next_LeaderFindsTheOldMatchStillClosing_KeepsWaitingToRejoin()
        {
            Assert.That(Next(PartyRole.Leader, PartyReturnStep.Join, SessionStartOutcome.Refused, 2f), Is.EqualTo(PartyReturnStep.Join));
        }

        [Test]
        public void RestartsWait_PartyStillInItsMatch_IsTrue()
        {
            Assert.That(PartyReturnRules.RestartsWait(SessionStartOutcome.Refused), Is.True);
        }

        [TestCase(SessionStartOutcome.NotFound)]
        [TestCase(SessionStartOutcome.NameTaken)]
        public void RestartsWait_PartyGoneOrTaken_IsFalse(SessionStartOutcome outcome)
        {
            Assert.That(PartyReturnRules.RestartsWait(outcome), Is.False);
        }

        [Test]
        public void RoleAfterLeaderLeft_LongestStandingMember_Leads()
        {
            var party = PartyOf(Anna, Boris, Vera);

            Assert.That(PartyReturnRules.RoleAfterLeaderLeft(party, Boris), Is.EqualTo(PartyRole.Leader));
            Assert.That(PartyReturnRules.RoleAfterLeaderLeft(party, Vera), Is.EqualTo(PartyRole.Member));
        }

        [Test]
        public void RoleAfterLeaderLeft_LastKnownParty_IsLeftUnchanged()
        {
            var party = PartyOf(Anna, Boris);

            PartyReturnRules.RoleAfterLeaderLeft(party, Boris);

            Assert.That(party.LeaderId, Is.EqualTo(Anna));
        }

        private static PartyReturnStep Next(PartyRole role, PartyReturnStep last, SessionStartOutcome outcome, float elapsed)
        {
            return PartyReturnRules.Next(role, last, outcome, elapsed, RejoinSeconds, GiveUpSeconds);
        }

        private static PartyState PartyOf(params int[] ids)
        {
            var party = new PartyState(6);
            foreach (var id in ids)
            {
                party.Join(id, $"Member {id}");
            }

            return party;
        }
    }
}
