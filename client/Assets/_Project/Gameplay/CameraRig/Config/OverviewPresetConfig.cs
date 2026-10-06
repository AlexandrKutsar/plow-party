using System;
using PlowParty.Gameplay.CameraRig.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Config
{
    [Serializable]
    public sealed class OverviewPresetConfig
    {
        [SerializeField, Range(10f, 90f)] private float _pitch = 60f;
        [SerializeField] private float _yaw;
        [SerializeField] private bool _orthographic;
        [SerializeField, Range(10f, 100f)] private float _fieldOfView = 40f;
        [SerializeField, Min(0f)] private float _padding = 1f;
        [SerializeField, Min(1f)] private float _orthographicDistance = 60f;

        public OverviewSettings ToSettings()
        {
            return new OverviewSettings
            {
                Pitch = _pitch,
                Yaw = _yaw,
                Lens = new CameraLens(_orthographic, _fieldOfView, 0f),
                Padding = _padding,
                OrthographicDistance = _orthographicDistance,
            };
        }
    }
}
