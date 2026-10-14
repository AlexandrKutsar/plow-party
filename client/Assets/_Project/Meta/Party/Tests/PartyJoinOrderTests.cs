using NUnit.Framework;
using PlowParty.Meta.Party.Simulation;

namespace PlowParty.Meta.Party.Tests
{
    public sealed class PartyJoinOrderTests
    {
        [Test]
        public void Merge_NothingRemembered_IsTheCurrentOrder()
        {
            var order = PartyJoinOrder.Merge(new string[0], new[] { "anna", "boris" });

            Assert.That(order, Is.EqualTo(new[] { "anna", "boris" }));
        }

        [Test]
        public void Merge_MemberNotBackYet_KeepsTheirPlace()
        {
            var order = PartyJoinOrder.Merge(new[] { "anna", "gleb", "boris" }, new[] { "anna", "boris" });

            Assert.That(order, Is.EqualTo(new[] { "anna", "gleb", "boris" }));
        }

        [Test]
        public void Merge_FormerLeaderNotBack_StandsRightBehindTheNewLeader()
        {
            var order = PartyJoinOrder.Merge(new[] { "anna", "boris" }, new[] { "boris", "gleb" });

            Assert.That(order, Is.EqualTo(new[] { "boris", "anna", "gleb" }));
        }
    }
}
