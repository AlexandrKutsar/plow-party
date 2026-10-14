using System;
using System.Collections.Generic;

namespace PlowParty.Shared
{
    public sealed class CustomGameSetup
    {
        private readonly CustomGameSlot[] _slots;

        public CustomGameSetup(IEnumerable<CustomGameSlot> slots)
        {
            _slots = new List<CustomGameSlot>(slots ?? throw new ArgumentNullException(nameof(slots))).ToArray();
            foreach (var slot in _slots)
            {
                if (slot.IsBot)
                {
                    BotCount++;
                }
                else
                {
                    PlayerCount++;
                }
            }

            if (PlayerCount < 1)
            {
                throw new ArgumentException("A Custom Game has at least one Player", nameof(slots));
            }
        }

        public IReadOnlyList<CustomGameSlot> Slots => _slots;

        public int PlayerCount { get; }

        public int BotCount { get; }

        public int SlotOfPlayer(int playerId)
        {
            for (var slot = 0; slot < _slots.Length; slot++)
            {
                if (!_slots[slot].IsBot && _slots[slot].PlayerId == playerId)
                {
                    return slot;
                }
            }

            return -1;
        }
    }
}
