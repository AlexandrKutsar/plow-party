using System;
using System.Collections.Generic;
using PlowParty.Shared;

namespace PlowParty.Gameplay.Match.Simulation
{
    public sealed class WaitingRules
    {
        private readonly MatchSettings _settings;

        public WaitingRules(MatchSettings settings)
        {
            _settings = settings;
        }

        public static bool AllSlotsFilled(SeatPlan plan, int players, int bots)
        {
            return players + bots >= plan.SlotCount;
        }

        public static bool HasFreeSlot(SeatPlan plan, int players, int bots)
        {
            return !AllSlotsFilled(plan, players, bots);
        }

        public SeatPlan PlanSeats(bool hasMatchmakingResult, MatchmakingResult matchmakingResult, int playersPresent, int slotLimit)
        {
            var slotCount = Math.Min(hasMatchmakingResult ? matchmakingResult.MaxSlots : _settings.FallbackSlotCount, slotLimit);
            var expectedPlayers = hasMatchmakingResult ? matchmakingResult.ExpectedHumans : Math.Max(1, playersPresent);
            return new SeatPlan(slotCount, Math.Min(expectedPlayers, slotCount));
        }

        public float[] ScheduleBotArrivals(int botCount, Random random)
        {
            var arrivals = new float[Math.Max(0, botCount)];
            var window = _settings.BotArrivalEnd - _settings.BotArrivalStart;
            for (var i = 0; i < arrivals.Length; i++)
            {
                arrivals[i] = _settings.BotArrivalStart + (float)random.NextDouble() * window;
            }

            Array.Sort(arrivals);
            return arrivals;
        }

        public int BotsToSeat(SeatPlan plan, int players, int bots, float waitingElapsed, IReadOnlyList<float> arrivals)
        {
            var free = plan.SlotCount - players - bots;
            if (free <= 0)
            {
                return 0;
            }

            if (waitingElapsed >= _settings.WaitingDuration)
            {
                return free;
            }

            var reservedForPlayers = Math.Max(0, plan.ExpectedPlayers - players);
            var botRoom = free - reservedForPlayers;
            var due = 0;
            while (due < arrivals.Count && arrivals[due] <= waitingElapsed)
            {
                due++;
            }

            return Math.Max(0, Math.Min(due - bots, botRoom));
        }
    }
}
