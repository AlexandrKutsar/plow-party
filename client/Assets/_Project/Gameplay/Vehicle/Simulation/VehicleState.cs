using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public readonly struct VehicleState
    {
        public VehicleState(Vector2 position, Vector2 velocity, Vector2 forward)
        {
            Position = position;
            Velocity = velocity;
            Forward = forward;
        }

        public Vector2 Position { get; }

        public Vector2 Velocity { get; }

        public Vector2 Forward { get; }

        public VehicleState WithVelocity(Vector2 velocity)
        {
            return new VehicleState(Position, velocity, Forward);
        }

        public static VehicleState At(Vector2 position, Vector2 forward)
        {
            return new VehicleState(position, Vector2.zero, forward.normalized);
        }
    }
}
