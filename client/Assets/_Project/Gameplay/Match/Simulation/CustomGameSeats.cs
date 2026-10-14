using System.Collections.Generic;
using PlowParty.Shared;

namespace PlowParty.Gameplay.Match.Simulation
{
    public static class CustomGameSeats
    {
        public static List<int> ReservedColors(CustomGameSetup setup, IReadOnlyCollection<int> seatedSlots)
        {
            var reserved = new List<int>(setup.PlayerCount);
            for (var slot = 0; slot < setup.Slots.Count; slot++)
            {
                if (!setup.Slots[slot].IsBot && !Contains(seatedSlots, slot))
                {
                    reserved.Add(setup.Slots[slot].Color);
                }
            }

            return reserved;
        }

        public static int BotSlot(CustomGameSetup setup, int botIndex)
        {
            var seen = 0;
            for (var slot = 0; slot < setup.Slots.Count; slot++)
            {
                if (!setup.Slots[slot].IsBot)
                {
                    continue;
                }

                if (seen == botIndex)
                {
                    return slot;
                }

                seen++;
            }

            return -1;
        }

        private static bool Contains(IReadOnlyCollection<int> values, int value)
        {
            foreach (var item in values)
            {
                if (item == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
