using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public readonly struct CameraTarget
    {
        public CameraTarget(Vector2 position, float height, Vector2 velocity, Vector2 forward)
        {
            Position = position;
            Height = height;
            Velocity = velocity;
            Forward = forward;
        }

        public Vector2 Position { get; }

        public float Height { get; }

        public Vector2 Velocity { get; }

        public Vector2 Forward { get; }
    }
}
