using System;
using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Gameplay.Participants.Simulation;

namespace PlowParty.Gameplay.Participants.Tests
{
    public sealed class ParticipantProfilesTests
    {
        [Test]
        public void PlayerNickname_TokenNickname_IsTrimmedAndKept()
        {
            Assert.That(ParticipantProfiles.PlayerNickname("  Frosty  ", 2), Is.EqualTo("Frosty"));
        }

        [Test]
        public void PlayerNickname_NoNickname_FallsBackToSlotNumber()
        {
            Assert.That(ParticipantProfiles.PlayerNickname(null, 0), Is.EqualTo("Player 1"));
            Assert.That(ParticipantProfiles.PlayerNickname("   ", 3), Is.EqualTo("Player 4"));
        }

        [Test]
        public void PlayerNickname_TooLong_IsCutToMaxLength()
        {
            var nickname = ParticipantProfiles.PlayerNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 0);

            Assert.That(nickname, Is.EqualTo("ABCDEFGHIJKLMNOP"));
            Assert.That(nickname.Length, Is.EqualTo(ParticipantProfiles.MaxNicknameLength));
        }

        [Test]
        public void BotNickname_FreeNamesLeft_PicksAnUntakenOne()
        {
            var pool = new[] { "Ada", "Bob", "Cid" };
            var taken = new List<string> { "Ada", "Cid" };

            for (var seed = 0; seed < 20; seed++)
            {
                Assert.That(ParticipantProfiles.BotNickname(pool, taken, 4, new Random(seed)), Is.EqualTo("Bob"));
            }
        }

        [Test]
        public void BotNickname_EveryNameTaken_AddsANumberToStayUnique()
        {
            var pool = new[] { "Ada" };
            var taken = new List<string> { "Ada" };

            Assert.That(ParticipantProfiles.BotNickname(pool, taken, 4, new Random(1)), Is.EqualTo("Ada 2"));
        }

        [Test]
        public void BotNickname_EmptyPool_FallsBackToSlotNumber()
        {
            Assert.That(ParticipantProfiles.BotNickname(Array.Empty<string>(), new List<string>(), 4, new Random(1)), Is.EqualTo("Player 5"));
        }

        [Test]
        public void PickSpecies_SomeTaken_PicksAnUntakenOne()
        {
            var taken = new List<CritterSpecies> { CritterSpecies.Fox, CritterSpecies.Bear, CritterSpecies.Rabbit, CritterSpecies.Raccoon, CritterSpecies.Penguin };

            for (var seed = 0; seed < 20; seed++)
            {
                Assert.That(ParticipantProfiles.PickSpecies(taken, new Random(seed)), Is.EqualTo(CritterSpecies.Beaver));
            }
        }

        [Test]
        public void PickSpecies_AllTaken_StillPicksASpecies()
        {
            var taken = new List<CritterSpecies>((CritterSpecies[])Enum.GetValues(typeof(CritterSpecies)));

            var species = ParticipantProfiles.PickSpecies(taken, new Random(3));

            Assert.That(Enum.IsDefined(typeof(CritterSpecies), species), Is.True);
        }
    }
}
