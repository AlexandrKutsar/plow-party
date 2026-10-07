using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public readonly struct CameraShakeSample
    {
        public CameraShakeSample(Vector2 offset, float roll)
        {
            Offset = offset;
            Roll = roll;
        }

        public Vector2 Offset { get; }

        public float Roll { get; }
    }
}
