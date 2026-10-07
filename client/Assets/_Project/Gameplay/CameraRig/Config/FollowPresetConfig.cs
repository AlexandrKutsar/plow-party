using System;
using PlowParty.Gameplay.CameraRig.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Config
{
    [Serializable]
    public sealed class FollowPresetConfig
    {
        [SerializeField, Range(10f, 90f)] private float _pitch = 55f;
        [SerializeField] private float _yaw;
        [SerializeField] private bool _rotateWithVehicle;
        [SerializeField, Min(1f)] private float _distance = 14f;
        [SerializeField] private bool _orthographic;
        [SerializeField, Range(10f, 100f)] private float _fieldOfView = 40f;
        [SerializeField, Min(0.5f)] private float _orthographicSize = 7f;
        [SerializeField, Min(0f)] private float _followSmoothTime = 0.25f;
        [SerializeField, Min(0f)] private float _yawSmoothTime = 0.5f;
        [SerializeField, Min(0f)] private float _lookAheadTime = 0.4f;
        [SerializeField, Min(0f)] private float _maxLookAhead = 3f;
        [SerializeField] private float _edgeOverscan = 2f;
        [SerializeField, Min(0.1f)] private float _snapDistance = 3f;

        public FollowPresetConfig()
        {
        }

        public FollowPresetConfig(bool rotateWithVehicle)
        {
            _rotateWithVehicle = rotateWithVehicle;
        }

        public FollowSettings ToSettings()
        {
            return new FollowSettings
            {
                Pitch = _pitch,
                Yaw = _yaw,
                RotateWithVehicle = _rotateWithVehicle,
                Distance = _distance,
                Lens = new CameraLens(_orthographic, _fieldOfView, _orthographicSize),
                FollowSmoothTime = _followSmoothTime,
                YawSmoothTime = _yawSmoothTime,
                LookAheadTime = _lookAheadTime,
                MaxLookAhead = _maxLookAhead,
                EdgeOverscan = _edgeOverscan,
                SnapDistance = _snapDistance,
            };
        }
    }
}
