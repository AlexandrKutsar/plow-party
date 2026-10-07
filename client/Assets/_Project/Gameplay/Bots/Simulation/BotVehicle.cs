using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public readonly struct BotVehicle
    {
        public BotVehicle(int slot, Vector2 position, Vector2 velocity, Vector2 forward, int load)
        {
            Slot = slot;
            Position = position;
            Velocity = velocity;
            Forward = forward;
            Load = load;
        }

        public int Slot { get; }

        public Vector2 Position { get; }

        public Vector2 Velocity { get; }

        public Vector2 Forward { get; }

        public int Load { get; }
    }
}
