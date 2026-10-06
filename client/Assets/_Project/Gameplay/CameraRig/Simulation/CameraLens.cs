namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public readonly struct CameraLens
    {
        public CameraLens(bool orthographic, float fieldOfView, float orthographicSize)
        {
            Orthographic = orthographic;
            FieldOfView = fieldOfView;
            OrthographicSize = orthographicSize;
        }

        public bool Orthographic { get; }

        public float FieldOfView { get; }

        public float OrthographicSize { get; }

        public CameraLens WithOrthographicSize(float size)
        {
            return new CameraLens(Orthographic, FieldOfView, size);
        }
    }
}
