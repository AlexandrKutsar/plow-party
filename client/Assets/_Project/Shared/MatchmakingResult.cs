using System;

namespace PlowParty.Shared
{
    public readonly struct MatchmakingResult
    {
        public MatchmakingResult(int expectedHumans, int maxSlots, PartyMode mode = PartyMode.QuickPlay)
        {
            if (maxSlots < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSlots), maxSlots, "At least one Slot");
            }

            if (expectedHumans < 1 || expectedHumans > maxSlots)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedHumans), expectedHumans, $"Between 1 and {maxSlots}");
            }

            ExpectedHumans = expectedHumans;
            MaxSlots = maxSlots;
            Mode = mode;
            Setup = null;
        }

        public MatchmakingResult(CustomGameSetup setup)
        {
            Setup = setup ?? throw new ArgumentNullException(nameof(setup));
            ExpectedHumans = setup.PlayerCount;
            MaxSlots = setup.Slots.Count;
            Mode = PartyMode.CustomGame;
        }

        public int ExpectedHumans { get; }
        public int MaxSlots { get; }
        public PartyMode Mode { get; }
        public CustomGameSetup Setup { get; }
    }
}
