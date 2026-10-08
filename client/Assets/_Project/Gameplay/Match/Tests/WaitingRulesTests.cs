using System;
using NUnit.Framework;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Shared;

namespace PlowParty.Gameplay.Match.Tests
{
    public sealed class WaitingRulesTests
    {
        private WaitingRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = MatchTestSettings.CreateWaitingRules();
        }

        [Test]
        public void PlanSeats_WithMatchmakingResult_ExpectsItsPlayersInItsSlots()
        {
            var plan = _rules.PlanSeats(true, new MatchmakingResult(3, 5), 1, 6);

            Assert.That(plan.SlotCount, Is.EqualTo(5));
            Assert.That(plan.ExpectedPlayers, Is.EqualTo(3));
            Assert.That(plan.PlannedBots, Is.EqualTo(2));
        }

        [Test]
        public void PlanSeats_MatchmakingResultBeyondSpawnPoints_IsClampedToSlotLimit()
        {
            var plan = _rules.PlanSeats(true, new MatchmakingResult(6, 6), 1, 4);

            Assert.That(plan.SlotCount, Is.EqualTo(4));
            Assert.That(plan.ExpectedPlayers, Is.EqualTo(4));
        }

        [Test]
        public void PlanSeats_NoMatchmakingResult_ExpectsPlayersPresentInFallbackSlots()
        {
            var plan = _rules.PlanSeats(false, default, 2, 6);

            Assert.That(plan.SlotCount, Is.EqualTo(6));
            Assert.That(plan.ExpectedPlayers, Is.EqualTo(2));
        }

        [Test]
        public void PlanSeats_NoMatchmakingResultAndNobodyYet_ExpectsTheHost()
        {
            Assert.That(_rules.PlanSeats(false, default, 0, 6).ExpectedPlayers, Is.EqualTo(1));
        }

        [Test]
        public void ScheduleBotArrivals_Count_SortedInsideTheWindow()
        {
            var arrivals = _rules.ScheduleBotArrivals(5, new Random(7));

            Assert.That(arrivals.Length, Is.EqualTo(5));
            for (var i = 0; i < arrivals.Length; i++)
            {
                Assert.That(arrivals[i], Is.InRange(1f, 8f));
                if (i > 0)
                {
                    Assert.That(arrivals[i], Is.GreaterThanOrEqualTo(arrivals[i - 1]));
                }
            }
        }

        [Test]
        public void ScheduleBotArrivals_NoBots_IsEmpty()
        {
            Assert.That(_rules.ScheduleBotArrivals(0, new Random(7)), Is.Empty);
        }

        [Test]
        public void BotsToSeat_BeforeFirstArrival_IsZero()
        {
            var plan = new SeatPlan(6, 1);

            Assert.That(_rules.BotsToSeat(plan, 1, 0, 1.5f, new[] { 2f, 4f, 6f, 7f, 8f }), Is.EqualTo(0));
        }

        [Test]
        public void BotsToSeat_ArrivalsDue_SeatsOnlyThoseNotSeatedYet()
        {
            var plan = new SeatPlan(6, 1);

            Assert.That(_rules.BotsToSeat(plan, 1, 1, 5f, new[] { 2f, 4f, 6f, 7f, 8f }), Is.EqualTo(1));
        }

        [Test]
        public void BotsToSeat_ExpectedPlayersMissing_KeepsTheirSlotsFree()
        {
            var plan = new SeatPlan(6, 3);

            Assert.That(_rules.BotsToSeat(plan, 1, 3, 9f, new[] { 2f, 4f, 6f }), Is.EqualTo(0));
        }

        [Test]
        public void BotsToSeat_ExtraPlayerArrived_TakesASlotFromTheBots()
        {
            var plan = new SeatPlan(6, 1);

            Assert.That(_rules.BotsToSeat(plan, 2, 4, 9f, new[] { 2f, 3f, 4f, 5f, 6f }), Is.EqualTo(0));
        }

        [Test]
        public void BotsToSeat_WaitingCapReached_FillsEveryFreeSlot()
        {
            var plan = new SeatPlan(6, 3);

            Assert.That(_rules.BotsToSeat(plan, 1, 3, 15f, new[] { 2f, 4f, 6f }), Is.EqualTo(2));
        }

        [Test]
        public void BotsToSeat_CapReachedWithEveryExpectedPlayerSeated_KeepsTheSchedule()
        {
            var plan = new SeatPlan(6, 2);

            Assert.That(_rules.BotsToSeat(plan, 2, 1, 15f, new[] { 14f, 15f, 16f, 16.5f }), Is.EqualTo(1));
        }

        [Test]
        public void AllExpectedPlayersSeated_SomeoneStillLoading_IsFalse()
        {
            var plan = new SeatPlan(6, 3);

            Assert.That(WaitingRules.AllExpectedPlayersSeated(plan, 2), Is.False);
            Assert.That(WaitingRules.AllExpectedPlayersSeated(plan, 3), Is.True);
            Assert.That(WaitingRules.AllExpectedPlayersSeated(plan, 4), Is.True);
        }

        [Test]
        public void HurryBotArrivals_EveryPlayerSeated_RemainingBotsArriveInTheQuickWindowFromNow()
        {
            var hurried = _rules.HurryBotArrivals(new[] { 2f, 6f, 7f, 8f }, 1, 3f, new Random(7));

            Assert.That(hurried.Length, Is.EqualTo(4));
            Assert.That(hurried[0], Is.EqualTo(2f));
            for (var i = 1; i < hurried.Length; i++)
            {
                Assert.That(hurried[i], Is.InRange(3.5f, 5.5f));
                Assert.That(hurried[i], Is.GreaterThanOrEqualTo(hurried[i - 1]));
            }
        }

        [Test]
        public void HurryBotArrivals_HurriedSchedule_SeatsEveryBotWithinTwoAndAHalfSeconds()
        {
            var plan = new SeatPlan(6, 1);
            var hurried = _rules.HurryBotArrivals(_rules.ScheduleBotArrivals(5, new Random(3)), 0, 0f, new Random(11));

            Assert.That(_rules.BotsToSeat(plan, 1, 0, 2.5f, hurried), Is.EqualTo(5));
        }

        [Test]
        public void BotsToSeat_AllSlotsFilled_IsZeroEvenAtTheCap()
        {
            var plan = new SeatPlan(4, 2);

            Assert.That(_rules.BotsToSeat(plan, 2, 2, 20f, new[] { 2f, 4f }), Is.EqualTo(0));
        }

        [Test]
        public void AllSlotsFilled_PlayersAndBotsTogether_CountAgainstSlots()
        {
            var plan = new SeatPlan(6, 2);

            Assert.That(WaitingRules.AllSlotsFilled(plan, 2, 3), Is.False);
            Assert.That(WaitingRules.AllSlotsFilled(plan, 2, 4), Is.True);
        }

        [Test]
        public void HasFreeSlot_FullOrNot_ReportsRoomForAnotherParticipant()
        {
            var plan = new SeatPlan(4, 2);

            Assert.That(WaitingRules.HasFreeSlot(plan, 2, 1), Is.True);
            Assert.That(WaitingRules.HasFreeSlot(plan, 3, 1), Is.False);
        }
    }
}
