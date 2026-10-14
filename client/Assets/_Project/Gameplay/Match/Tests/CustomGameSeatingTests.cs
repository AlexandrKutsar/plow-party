using NUnit.Framework;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Shared;

namespace PlowParty.Gameplay.Match.Tests
{
    public sealed class CustomGameSeatingTests
    {
        private const int Anna = 11;
        private const int Boris = 12;

        private WaitingRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = MatchTestSettings.CreateWaitingRules();
        }

        [Test]
        public void PlanSeats_CustomGame_SeatsExactlyItsPlayersAndBots()
        {
            var plan = _rules.PlanSeats(true, new MatchmakingResult(Setup()), 1, 6);

            Assert.That(plan.SlotCount, Is.EqualTo(4));
            Assert.That(plan.ExpectedPlayers, Is.EqualTo(2));
            Assert.That(plan.PlannedBots, Is.EqualTo(2));
            Assert.That(plan.ReplacesMissingPlayers, Is.False);
        }

        [Test]
        public void PlanSeats_QuickPlay_ReplacesMissingPlayers()
        {
            Assert.That(_rules.PlanSeats(true, new MatchmakingResult(1, 6), 1, 6).ReplacesMissingPlayers, Is.True);
        }

        [Test]
        public void BotsToSeat_CustomGameAtTheCapWithAPlayerMissing_SeatsOnlyTheLeadersBots()
        {
            var plan = new SeatPlan(4, 2, false);

            Assert.That(_rules.BotsToSeat(plan, 1, 1, 15f, new[] { 20f, 30f }), Is.EqualTo(1));
        }

        [Test]
        public void IsWaitingOver_CustomGameAtTheCapWithAPlayerMissing_StartsWithWhoeverIsSeated()
        {
            var plan = new SeatPlan(4, 2, false);

            Assert.That(_rules.IsWaitingOver(plan, 1, 2, 14f), Is.False);
            Assert.That(_rules.IsWaitingOver(plan, 1, 2, 15f), Is.True);
        }

        [Test]
        public void IsWaitingOver_QuickPlayAtTheCapWithFreeSlots_KeepsWaitingForBots()
        {
            var plan = new SeatPlan(4, 2);

            Assert.That(_rules.IsWaitingOver(plan, 1, 2, 15f), Is.False);
            Assert.That(_rules.IsWaitingOver(plan, 1, 3, 15f), Is.True);
        }

        [Test]
        public void ReservedColors_PicksOfPlayersNotSeatedYet_AreKeptFromBots()
        {
            var reserved = CustomGameSeats.ReservedColors(Setup(), new[] { 0 });

            Assert.That(reserved, Is.EquivalentTo(new[] { 2 }));
        }

        [Test]
        public void BotSlot_NthBot_TakesTheNthBotSlotOfTheSetup()
        {
            Assert.That(CustomGameSeats.BotSlot(Setup(), 0), Is.EqualTo(1));
            Assert.That(CustomGameSeats.BotSlot(Setup(), 1), Is.EqualTo(3));
            Assert.That(CustomGameSeats.BotSlot(Setup(), 2), Is.EqualTo(-1));
        }

        private static CustomGameSetup Setup()
        {
            return new CustomGameSetup(new[]
            {
                CustomGameSlot.Player(Anna, 5, 1),
                CustomGameSlot.Bot(BotDifficulty.Weak),
                CustomGameSlot.Player(Boris, 2, 1),
                CustomGameSlot.Bot(BotDifficulty.Strong),
            });
        }
    }
}
