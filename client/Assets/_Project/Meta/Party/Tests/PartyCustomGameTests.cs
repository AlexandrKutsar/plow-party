using System.Linq;
using NUnit.Framework;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Party.Tests
{
    public sealed class PartyCustomGameTests
    {
        private const int Capacity = 6;
        private const int Anna = 1;
        private const int Boris = 2;
        private const int Vera = 3;

        [Test]
        public void Join_EachMember_GetsTheirOwnSlotAndAUniqueColor()
        {
            var party = PartyOf(Anna, Boris, Vera);

            Assert.That(party.Members.Select(member => member.Slot), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(party.Members.Select(member => member.Color).Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void SetBot_LeaderOnEmptySlot_PutsABotWithThatDifficulty()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.SetBot(Anna, 4, BotDifficulty.Strong), Is.True);

            Assert.That(party.Bots.Single().Slot, Is.EqualTo(4));
            Assert.That(party.Bots.Single().Difficulty, Is.EqualTo(BotDifficulty.Strong));
        }

        [Test]
        public void SetBot_OnABotSlot_ChangesItsDifficulty()
        {
            var party = PartyOf(Anna);
            party.SetBot(Anna, 2, BotDifficulty.Weak);

            party.SetBot(Anna, 2, BotDifficulty.Medium);

            Assert.That(party.Bots.Single().Difficulty, Is.EqualTo(BotDifficulty.Medium));
        }

        [Test]
        public void SetBot_ByAMember_IsRefused()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.SetBot(Boris, 3, BotDifficulty.Weak), Is.False);
            Assert.That(party.Bots, Is.Empty);
        }

        [Test]
        public void SetBot_OnAMembersSlot_IsRefused()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.SetBot(Anna, 1, BotDifficulty.Weak), Is.False);
        }

        [Test]
        public void RemoveBot_Leader_LeavesTheSlotEmpty()
        {
            var party = PartyOf(Anna);
            party.SetBot(Anna, 3, BotDifficulty.Weak);

            Assert.That(party.RemoveBot(Anna, 3), Is.True);
            Assert.That(party.Bots, Is.Empty);
        }

        [Test]
        public void Join_EverySlotTaken_NewMemberReplacesTheLastBot()
        {
            var party = PartyOf(Anna);
            for (var slot = 1; slot < Capacity; slot++)
            {
                party.SetBot(Anna, slot, BotDifficulty.Medium);
            }

            party.Join(Boris, "Boris");

            Assert.That(party.Members[1].Slot, Is.EqualTo(5));
            Assert.That(party.Bots.Count, Is.EqualTo(4));
        }

        [Test]
        public void PickColor_FreeColor_IsTaken()
        {
            var party = PartyOf(Anna, Boris);

            Assert.That(party.PickColor(Boris, 6), Is.True);
            Assert.That(party.Members[1].Color, Is.EqualTo(6));
        }

        [Test]
        public void PickColor_HeldByAnotherMember_IsRefused()
        {
            var party = PartyOf(Anna, Boris);
            var annasColor = party.Members[0].Color;

            Assert.That(party.PickColor(Boris, annasColor), Is.False);
            Assert.That(party.Members[1].Color, Is.Not.EqualTo(annasColor));
        }

        [Test]
        public void PickColor_OutsideThePalette_IsRefused()
        {
            var party = PartyOf(Anna);

            Assert.That(party.PickColor(Anna, PartyLooks.DefaultColorCount), Is.False);
        }

        [Test]
        public void PickCritter_SameCritterAsAnotherMember_IsAllowed()
        {
            var party = PartyOf(Anna, Boris);
            party.PickCritter(Anna, 3);

            Assert.That(party.PickCritter(Boris, 3), Is.True);
            Assert.That(party.Members[1].Critter, Is.EqualTo(3));
        }

        [Test]
        public void IsColorTaken_ColorOfAnotherMember_IsTakenForMeButNotForThem()
        {
            var party = PartyOf(Anna, Boris);
            var borisColor = party.Members[1].Color;

            Assert.That(party.IsColorTaken(borisColor, Anna), Is.True);
            Assert.That(party.IsColorTaken(borisColor, Boris), Is.False);
        }

        [Test]
        public void CustomGameSetup_MembersAndBots_InSlotOrderWithoutEmptySlots()
        {
            var party = PartyOf(Anna, Boris);
            party.PickColor(Anna, 5);
            party.PickCritter(Boris, 4);
            party.SetBot(Anna, 4, BotDifficulty.Strong);
            party.SetBot(Anna, 2, BotDifficulty.Weak);

            var setup = party.CustomGameSetup();

            Assert.That(setup.PlayerCount, Is.EqualTo(2));
            Assert.That(setup.BotCount, Is.EqualTo(2));
            Assert.That(setup.Slots[0].PlayerId, Is.EqualTo(Anna));
            Assert.That(setup.Slots[0].Color, Is.EqualTo(5));
            Assert.That(setup.Slots[1].PlayerId, Is.EqualTo(Boris));
            Assert.That(setup.Slots[1].Critter, Is.EqualTo(4));
            Assert.That(setup.Slots[2].Difficulty, Is.EqualTo(BotDifficulty.Weak));
            Assert.That(setup.Slots[3].Difficulty, Is.EqualTo(BotDifficulty.Strong));
        }

        [Test]
        public void CustomGameSetup_SoloPlayer_IsSeatedUnderTheGivenPlayerId()
        {
            var party = PartyOf(PartyState.SoloId);
            party.SetBot(PartyState.SoloId, 1, BotDifficulty.Medium);

            var setup = party.CustomGameSetup(PartyState.SoloId, 42);

            Assert.That(setup.SlotOfPlayer(42), Is.EqualTo(0));
            Assert.That(setup.BotCount, Is.EqualTo(1));
        }

        [Test]
        public void Restore_PublishedSetup_KeepsSlotsColorsCrittersAndBots()
        {
            var original = PartyOf(Anna, Boris);
            original.PickColor(Boris, 7);
            original.PickCritter(Boris, 2);
            original.SetBot(Anna, 3, BotDifficulty.Strong);

            var restored = PartyState.Restore(Capacity, PartyMode.CustomGame, original.Members, false, original.Bots);

            Assert.That(restored.Members[1].Slot, Is.EqualTo(1));
            Assert.That(restored.Members[1].Color, Is.EqualTo(7));
            Assert.That(restored.Members[1].Critter, Is.EqualTo(2));
            Assert.That(restored.Bots.Single().Slot, Is.EqualTo(3));
        }

        [Test]
        public void SetMode_CustomGame_LeaderCanStartWithEveryoneReady()
        {
            var party = PartyOf(Anna, Boris);
            party.SetMode(Anna, PartyMode.CustomGame);
            party.SetReady(Boris, true);

            Assert.That(party.Mode, Is.EqualTo(PartyMode.CustomGame));
            Assert.That(party.CanStart, Is.True);
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
