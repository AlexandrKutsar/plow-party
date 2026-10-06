using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public sealed class CameraShake : ICameraShake
    {
        private const float HorizontalNoiseRow = 0.5f;
        private const float VerticalNoiseRow = 7.5f;
        private const float RollNoiseRow = 15.5f;

        private float _time;

        public float Trauma { get; private set; }

        public void Add(float strength)
        {
            Trauma = Mathf.Clamp01(Trauma + Mathf.Max(0f, strength));
        }

        public CameraShakeSample Step(float deltaTime, ShakeSettings settings)
        {
            Trauma = Mathf.Max(0f, Trauma - settings.DecayPerSecond * deltaTime);
            _time += deltaTime;
            var amount = Trauma * Trauma;
            if (amount <= 0f)
            {
                return default;
            }

            var phase = _time * settings.Frequency;
            var offset = new Vector2(Noise(HorizontalNoiseRow, phase), Noise(VerticalNoiseRow, phase)) * (settings.MaxOffset * amount);
            return new CameraShakeSample(offset, Noise(RollNoiseRow, phase) * settings.MaxRoll * amount);
        }

        private static float Noise(float row, float phase)
        {
            return Mathf.Clamp(Mathf.PerlinNoise(row, phase) * 2f - 1f, -1f, 1f);
        }
    }
}
