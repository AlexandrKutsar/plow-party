using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Vehicle Config", fileName = nameof(VehicleConfig))]
    public sealed class VehicleConfig : ScriptableObject
    {
        [SerializeField] private float _radius = 0.54f;
        [SerializeField] private float _halfLength = 0.27f;
        [SerializeField] private float _maxSpeed = 8f;
        [SerializeField] private float _acceleration = 18f;
        [SerializeField] private float _deceleration = 9f;
        [SerializeField] private float _turnRateDegrees = 270f;
        [SerializeField] private float _restitution = 0.7f;
        [SerializeField] private float _ramMinAngleDegrees = 60f;
        [SerializeField] private float _ramMinSpeed = 3f;
        [SerializeField] private float _ramCooldown = 1f;
        [SerializeField] private float _ramKnockback = 5f;
        [SerializeField] private float _ramRecoil = 2f;

        public VehicleSettings ToSettings()
        {
            return new VehicleSettings
            {
                Radius = _radius,
                HalfLength = _halfLength,
                MaxSpeed = _maxSpeed,
                Acceleration = _acceleration,
                Deceleration = _deceleration,
                TurnRateDegrees = _turnRateDegrees,
                Restitution = _restitution,
                RamMinAngleDegrees = _ramMinAngleDegrees,
                RamMinSpeed = _ramMinSpeed,
                RamCooldown = _ramCooldown,
                RamKnockback = _ramKnockback,
                RamRecoil = _ramRecoil,
            };
        }
    }
}
