using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Session.Tests
{
    public sealed class LobbyPickTests
    {
        private const long Now = 100_000L;
        private const float MinSecondsBeforeStart = 3f;
        private const int MaxSlots = 6;

        [Test]
        public void Candidates_NoRoomForTheWholeParty_IsSkipped()
        {
            var candidates = Candidates(3, Lobby("crowded", 4, Now + 8_000L), Lobby("roomy", 3, Now + 8_000L));

            Assert.That(Names(candidates), Is.EqualTo(new[] { "roomy" }));
        }

        [Test]
        public void Candidates_StartsInLessThanThreeSeconds_IsSkipped()
        {
            var candidates = Candidates(1, Lobby("late", 5, Now + 2_500L), Lobby("early", 1, Now + 3_000L));

            Assert.That(Names(candidates), Is.EqualTo(new[] { "early" }));
        }

        [Test]
        public void Candidates_StartUnknown_IsSkipped()
        {
            var candidates = Candidates(1, Lobby("unknown", 2, 0L));

            Assert.That(candidates, Is.Empty);
        }

        [Test]
        public void Candidates_SeveralFit_MostPlayersFirst()
        {
            var candidates = Candidates(1, Lobby("two", 2, Now + 5_000L), Lobby("four", 4, Now + 9_000L), Lobby("one", 1, Now + 4_000L));

            Assert.That(Names(candidates), Is.EqualTo(new[] { "four", "two", "one" }));
        }

        [Test]
        public void Candidates_SamePlayers_SoonestStartFirst()
        {
            var candidates = Candidates(1, Lobby("later", 3, Now + 9_000L), Lobby("sooner", 3, Now + 4_000L));

            Assert.That(Names(candidates), Is.EqualTo(new[] { "sooner", "later" }));
        }

        [Test]
        public void TryMergeTarget_OlderLobbyFits_MovesThere()
        {
            var own = Lobby("own", 2, Now + 9_000L, Now - 1_000L);
            var older = Lobby("older", 1, Now + 7_000L, Now - 3_000L);

            Assert.That(LobbyPick.TryMergeTarget(new[] { own, older }, own, 2, Now, MinSecondsBeforeStart, out var target), Is.True);
            Assert.That(target.Name, Is.EqualTo("older"));
        }

        [Test]
        public void TryMergeTarget_OnlyNewerLobbies_StaysPut()
        {
            var own = Lobby("own", 1, Now + 7_000L, Now - 3_000L);
            var newer = Lobby("newer", 3, Now + 9_000L, Now - 1_000L);

            Assert.That(LobbyPick.TryMergeTarget(new[] { own, newer }, own, 1, Now, MinSecondsBeforeStart, out _), Is.False);
        }

        [Test]
        public void TryMergeTarget_StrangersAlreadyJoined_StaysPut()
        {
            var own = Lobby("own", 3, Now + 9_000L, Now - 1_000L);
            var older = Lobby("older", 1, Now + 7_000L, Now - 3_000L);

            Assert.That(LobbyPick.TryMergeTarget(new[] { own, older }, own, 2, Now, MinSecondsBeforeStart, out _), Is.False);
        }

        [Test]
        public void TryMergeTarget_OlderLobbyWithoutRoom_StaysPut()
        {
            var own = Lobby("own", 2, Now + 9_000L, Now - 1_000L);
            var older = Lobby("older", 5, Now + 7_000L, Now - 3_000L);

            Assert.That(LobbyPick.TryMergeTarget(new[] { own, older }, own, 2, Now, MinSecondsBeforeStart, out _), Is.False);
        }

        [Test]
        public void TryMergeTarget_SeveralOlderFit_PicksMostPlayers()
        {
            var own = Lobby("own", 1, Now + 9_000L, Now - 1_000L);
            var small = Lobby("small", 1, Now + 5_000L, Now - 5_000L);
            var big = Lobby("big", 3, Now + 7_000L, Now - 3_000L);

            LobbyPick.TryMergeTarget(new[] { own, small, big }, own, 1, Now, MinSecondsBeforeStart, out var target);

            Assert.That(target.Name, Is.EqualTo("big"));
        }

        [Test]
        public void TryMergeTarget_TwoLobbiesOpenedAtOnce_ExactlyOneMoves()
        {
            var first = Lobby("a", 1, Now + 9_000L, Now - 1_000L);
            var second = Lobby("b", 1, Now + 9_000L, Now - 1_000L);
            var listings = new[] { first, second };

            var firstMoves = LobbyPick.TryMergeTarget(listings, first, 1, Now, MinSecondsBeforeStart, out _);
            var secondMoves = LobbyPick.TryMergeTarget(listings, second, 1, Now, MinSecondsBeforeStart, out _);

            Assert.That(firstMoves, Is.Not.EqualTo(secondMoves));
        }

        private static IReadOnlyList<LobbyListing> Candidates(int partySize, params LobbyListing[] listings)
        {
            return LobbyPick.Candidates(listings, partySize, Now, MinSecondsBeforeStart);
        }

        private static LobbyListing Lobby(string name, int players, long startsAt, long openedAt = Now - 1_000L)
        {
            return new LobbyListing(name, players, MaxSlots, startsAt, openedAt);
        }

        private static string[] Names(IReadOnlyList<LobbyListing> listings)
        {
            var names = new string[listings.Count];
            for (var i = 0; i < listings.Count; i++)
            {
                names[i] = listings[i].Name;
            }

            return names;
        }
    }
}
