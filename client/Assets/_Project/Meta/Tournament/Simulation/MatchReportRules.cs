using System;
using System.Collections.Generic;
using PlowParty.Meta.Tournament.Api;

namespace PlowParty.Meta.Tournament.Simulation
{
    public static class MatchReportRules
    {
        public const int MinRosterSlots = 4;
        public const int MaxRosterSlots = 6;
        public const int MatchSeconds = 180;

        public static RegisterMatchRequest BuildRoster(IEnumerable<RosterSeat> seats)
        {
            var bySlot = new SortedDictionary<int, string>();
            var seated = new HashSet<string>();
            foreach (var seat in seats)
            {
                if (seat.Slot < 0 || seat.Slot >= MaxRosterSlots || bySlot.ContainsKey(seat.Slot))
                {
                    continue;
                }

                var account = Guid.TryParse(seat.AccountId, out _) && seated.Add(seat.AccountId) ? seat.AccountId : null;
                bySlot.Add(seat.Slot, account);
            }

            var request = new RegisterMatchRequest();
            foreach (var entry in bySlot)
            {
                request.Roster.Add(new RosterSlotRequest { Slot = entry.Key, AccountId = entry.Value });
            }

            return request;
        }

        public static bool CanRegister(RegisterMatchRequest roster, string hostAccountId)
        {
            var count = roster.Roster.Count;
            if (count < MinRosterSlots || count > MaxRosterSlots || string.IsNullOrEmpty(hostAccountId))
            {
                return false;
            }

            foreach (var slot in roster.Roster)
            {
                if (slot.AccountId == hostAccountId)
                {
                    return true;
                }
            }

            return false;
        }

        public static int SlotMask(IEnumerable<int> slots)
        {
            var mask = 0;
            foreach (var slot in slots)
            {
                mask |= 1 << slot;
            }

            return mask;
        }

        public static List<int> SlotsOf(int mask)
        {
            var slots = new List<int>();
            for (var slot = 0; slot < MaxRosterSlots; slot++)
            {
                if ((mask & (1 << slot)) != 0)
                {
                    slots.Add(slot);
                }
            }

            return slots;
        }

        public static VoteRequest BuildVote(IEnumerable<int> rosterSlots, IReadOnlyDictionary<int, int> scoresBySlot, int? interruptedAtSeconds)
        {
            var vote = new VoteRequest { InterruptedAtSeconds = interruptedAtSeconds };
            foreach (var slot in rosterSlots)
            {
                var score = scoresBySlot.TryGetValue(slot, out var value) ? Math.Max(0, value) : 0;
                vote.Scores.Add(new SlotScore { Slot = slot, Score = score });
            }

            return vote;
        }

        public static bool TryGetInterruptedSecond(float playingElapsed, out int second)
        {
            second = (int)Math.Floor(playingElapsed);
            return second >= 1 && second < MatchSeconds;
        }
    }
}
