using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public readonly struct CameraPose
    {
        public CameraPose(Vector3 position, Quaternion rotation, CameraLens lens)
        {
            Position = position;
            Rotation = rotation;
            Lens = lens;
        }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public CameraLens Lens { get; }
    }
}
