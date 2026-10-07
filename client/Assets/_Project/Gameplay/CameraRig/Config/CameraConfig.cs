using PlowParty.Gameplay.CameraRig.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Camera Config", fileName = nameof(CameraConfig))]
    public sealed class CameraConfig : ScriptableObject
    {
        [SerializeField] private CameraPreset _defaultPreset = CameraPreset.Follow;
        [SerializeField] private OverviewPresetConfig _overview = new OverviewPresetConfig();
        [SerializeField] private FollowPresetConfig _follow = new FollowPresetConfig(false);
        [SerializeField] private FollowPresetConfig _followRotating = new FollowPresetConfig(true);
        [SerializeField, Min(0f)] private float _shakeMaxOffset = 0.35f;
        [SerializeField, Min(0f)] private float _shakeMaxRoll = 2f;
        [SerializeField, Min(0f)] private float _shakeFrequency = 18f;
        [SerializeField, Min(0f)] private float _shakeDecayPerSecond = 1.5f;
        [SerializeField, Range(0f, 1f)] private float _ramVictimShake = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _ramRammerShake = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _pilePlowShake = 0.4f;

        public CameraPreset DefaultPreset => _defaultPreset;

        public float RamVictimShake => _ramVictimShake;

        public float RamRammerShake => _ramRammerShake;

        public float PilePlowShake => _pilePlowShake;

        public OverviewSettings ToOverviewSettings()
        {
            return _overview.ToSettings();
        }

        public FollowSettings ToFollowSettings(CameraPreset preset)
        {
            return preset == CameraPreset.FollowRotating ? _followRotating.ToSettings() : _follow.ToSettings();
        }

        public ShakeSettings ToShakeSettings()
        {
            return new ShakeSettings
            {
                MaxOffset = _shakeMaxOffset,
                MaxRoll = _shakeMaxRoll,
                Frequency = _shakeFrequency,
                DecayPerSecond = _shakeDecayPerSecond,
            };
        }
    }
}
