namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public struct FollowSettings
    {
        public float Pitch { get; set; }

        public float Yaw { get; set; }

        public bool RotateWithVehicle { get; set; }

        public float Distance { get; set; }

        public CameraLens Lens { get; set; }

        public float FollowSmoothTime { get; set; }

        public float YawSmoothTime { get; set; }

        public float LookAheadTime { get; set; }

        public float MaxLookAhead { get; set; }

        public float EdgeOverscan { get; set; }

        public float SnapDistance { get; set; }

        public CameraView ViewAt(float yaw)
        {
            return new CameraView(Pitch, yaw, Distance, Lens);
        }
    }
}
