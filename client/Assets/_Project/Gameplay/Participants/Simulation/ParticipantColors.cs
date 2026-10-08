using System;
using System.Collections.Generic;

namespace PlowParty.Gameplay.Participants.Simulation
{
    public static class ParticipantColors
    {
        public const int NoPreference = -1;

        public static int Assign(int preferred, IReadOnlyCollection<int> taken, int paletteSize, Random random)
        {
            if (paletteSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(paletteSize), paletteSize, "At least one colour");
            }

            if (preferred >= 0 && preferred < paletteSize)
            {
                return NearestFree(preferred, taken, paletteSize, random);
            }

            return RandomFree(taken, paletteSize, random);
        }

        private static int NearestFree(int preferred, IReadOnlyCollection<int> taken, int paletteSize, Random random)
        {
            for (var distance = 0; distance <= paletteSize / 2; distance++)
            {
                var next = (preferred + distance) % paletteSize;
                if (!Contains(taken, next))
                {
                    return next;
                }

                var previous = (preferred - distance + paletteSize) % paletteSize;
                if (!Contains(taken, previous))
                {
                    return previous;
                }
            }

            return RandomFree(taken, paletteSize, random);
        }

        private static int RandomFree(IReadOnlyCollection<int> taken, int paletteSize, Random random)
        {
            var free = new List<int>(paletteSize);
            for (var color = 0; color < paletteSize; color++)
            {
                if (!Contains(taken, color))
                {
                    free.Add(color);
                }
            }

            return free.Count > 0 ? free[random.Next(free.Count)] : random.Next(paletteSize);
        }

        private static bool Contains(IReadOnlyCollection<int> taken, int color)
        {
            foreach (var item in taken)
            {
                if (item == color)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
