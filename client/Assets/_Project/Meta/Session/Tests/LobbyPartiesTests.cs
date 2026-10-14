using NUnit.Framework;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Session.Tests
{
    public sealed class LobbyPartiesTests
    {
        [Test]
        public void Contains_LeaderArrivedInTheLobby_IsTrue()
        {
            var parties = LobbyParties.Format(new[] { "AB3XZ", "KM7PQ" });

            Assert.That(LobbyParties.Contains(parties, "KM7PQ"), Is.True);
        }

        [Test]
        public void Contains_OtherPartiesOnly_IsFalse()
        {
            var parties = LobbyParties.Format(new[] { "AB3XZ" });

            Assert.That(LobbyParties.Contains(parties, "KM7PQ"), Is.False);
        }

        [Test]
        public void Contains_NoPartyArrived_IsFalse()
        {
            Assert.That(LobbyParties.Contains(string.Empty, "KM7PQ"), Is.False);
        }

        [Test]
        public void TryFindLobbyOf_LeaderListedInALobby_FindsThatLobby()
        {
            var listings = new[]
            {
                new LobbyListing("strangers", 2, 6, 10_000L, 1_000L, LobbyParties.Format(new[] { "AB3XZ" })),
                new LobbyListing("leader", 3, 6, 10_000L, 1_000L, LobbyParties.Format(new[] { "KM7PQ" })),
            };

            Assert.That(LobbyParties.TryFindLobbyOf(listings, "KM7PQ", out var lobby), Is.True);
            Assert.That(lobby.Name, Is.EqualTo("leader"));
        }

        [Test]
        public void TryFindLobbyOf_LeaderNotInAnyLobbyYet_FindsNothing()
        {
            var listings = new[] { new LobbyListing("strangers", 2, 6, 10_000L, 1_000L, string.Empty) };

            Assert.That(LobbyParties.TryFindLobbyOf(listings, "KM7PQ", out _), Is.False);
        }
    }
}
