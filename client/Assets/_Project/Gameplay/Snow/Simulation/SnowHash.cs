namespace PlowParty.Gameplay.Snow.Simulation
{
    internal static class SnowHash
    {
        public static uint Mix(int seed, int first, int second)
        {
            var hash = unchecked((uint)seed * 0x9E3779B1u);
            hash ^= unchecked((uint)first * 0x85EBCA77u);
            hash = (hash << 13) | (hash >> 19);
            hash ^= unchecked((uint)second * 0xC2B2AE3Du);
            hash ^= hash >> 15;
            hash = unchecked(hash * 0x2C1B3C6Du);
            hash ^= hash >> 12;
            hash = unchecked(hash * 0x297A2D39u);
            hash ^= hash >> 15;
            return hash;
        }
    }
}
