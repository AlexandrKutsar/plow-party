namespace PlowParty.Gameplay.Match.Simulation
{
    public static class MatchPlacementRules
    {
        public static int Rank(int[] slots, int[] scores, int count, MatchPlacement[] output)
        {
            var ranked = count < output.Length ? count : output.Length;
            var filled = 0;
            for (var i = 0; i < count; i++)
            {
                filled = Insert(output, filled, ranked, new MatchPlacement(slots[i], scores[i], 0));
            }

            AssignPlaces(output, filled);
            return filled;
        }

        private static int Insert(MatchPlacement[] output, int filled, int capacity, MatchPlacement entry)
        {
            var index = filled;
            while (index > 0 && RanksAbove(entry, output[index - 1]))
            {
                if (index < capacity)
                {
                    output[index] = output[index - 1];
                }

                index--;
            }

            if (index < capacity)
            {
                output[index] = entry;
            }

            return filled < capacity ? filled + 1 : filled;
        }

        private static bool RanksAbove(MatchPlacement entry, MatchPlacement other)
        {
            return entry.Score > other.Score || (entry.Score == other.Score && entry.Slot < other.Slot);
        }

        private static void AssignPlaces(MatchPlacement[] output, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var place = i > 0 && output[i].Score == output[i - 1].Score ? output[i - 1].Place : i + 1;
                output[i] = new MatchPlacement(output[i].Slot, output[i].Score, place);
            }
        }
    }
}
