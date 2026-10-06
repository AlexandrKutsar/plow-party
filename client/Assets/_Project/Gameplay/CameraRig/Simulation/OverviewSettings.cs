namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public struct OverviewSettings
    {
        public float Pitch { get; set; }

        public float Yaw { get; set; }

        public CameraLens Lens { get; set; }

        public float Padding { get; set; }

        public float OrthographicDistance { get; set; }
    }
}
