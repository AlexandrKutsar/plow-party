namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public static class VehiclePairs
    {
        public static int Count(int capacity)
        {
            return capacity * (capacity - 1) / 2;
        }

        public static int Index(int first, int second)
        {
            var low = first < second ? first : second;
            var high = first < second ? second : first;
            return high * (high - 1) / 2 + low;
        }
    }
}
