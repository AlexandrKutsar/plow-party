using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.View
{
    internal sealed class SnowPileBursts
    {
        private readonly ParticleSystem _prefab;
        private readonly Transform _parent;
        private readonly float _bladeForwardOffset;
        private readonly ParticleSystem[] _bySlot = new ParticleSystem[VehicleWorldDriver.MaxVehicles];
        private readonly bool[] _seen = new bool[VehicleWorldDriver.MaxVehicles];

        public SnowPileBursts(ParticleSystem prefab, Transform parent, float bladeForwardOffset)
        {
            _prefab = prefab;
            _parent = parent;
            _bladeForwardOffset = bladeForwardOffset;
        }

        public void Show(VehicleRegistry registry, SnowGridDriver driver)
        {
            if (_prefab == null)
            {
                return;
            }

            System.Array.Clear(_seen, 0, _seen.Length);
            var vehicles = registry.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                if (vehicle.Slot < 0 || vehicle.Slot >= _bySlot.Length)
                {
                    continue;
                }

                _seen[vehicle.Slot] = true;
                var burst = BurstFor(vehicle.Slot);
                var body = vehicle.transform;
                burst.transform.SetPositionAndRotation(body.position + body.forward * _bladeForwardOffset, body.rotation);
                SetEmitting(burst, driver.IsPlowingPile(vehicle));
            }

            for (var slot = 0; slot < _bySlot.Length; slot++)
            {
                if (!_seen[slot] && _bySlot[slot] != null)
                {
                    SetEmitting(_bySlot[slot], false);
                }
            }
        }

        private static void SetEmitting(ParticleSystem burst, bool emitting)
        {
            var emission = burst.emission;
            emission.enabled = emitting;
        }

        private ParticleSystem BurstFor(int slot)
        {
            if (_bySlot[slot] == null)
            {
                _bySlot[slot] = Object.Instantiate(_prefab, _parent);
                _bySlot[slot].Play();
            }

            return _bySlot[slot];
        }
    }
}
