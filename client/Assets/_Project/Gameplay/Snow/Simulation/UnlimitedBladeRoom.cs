namespace PlowParty.Gameplay.Snow.Simulation
{
    public sealed class UnlimitedBladeRoom : IBladeRoom
    {
        public int GetRoom(int slot)
        {
            return int.MaxValue;
        }
    }
}
