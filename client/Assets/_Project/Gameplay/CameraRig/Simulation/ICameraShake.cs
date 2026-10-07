namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public interface ICameraShake
    {
        void Add(float strength);

        void Sustain(float strength);
    }
}
