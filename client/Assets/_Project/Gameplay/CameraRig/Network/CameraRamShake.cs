using System;
using PlowParty.Gameplay.CameraRig.Config;
using PlowParty.Gameplay.CameraRig.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using VContainer.Unity;

namespace PlowParty.Gameplay.CameraRig.Network
{
    public sealed class CameraRamShake : IStartable, IDisposable
    {
        private readonly VehicleRegistry _vehicles;
        private readonly ICameraShake _shake;
        private readonly CameraConfig _config;

        public CameraRamShake(VehicleRegistry vehicles, ICameraShake shake, CameraConfig config)
        {
            _vehicles = vehicles;
            _shake = shake;
            _config = config;
        }

        public void Start()
        {
            _vehicles.Rammed += OnRammed;
        }

        public void Dispose()
        {
            _vehicles.Rammed -= OnRammed;
        }

        private void OnRammed(VehicleRam ram)
        {
            if (ram.Victim.HasInputAuthority)
            {
                _shake.Add(_config.RamVictimShake);
            }
            else if (ram.Rammer.HasInputAuthority)
            {
                _shake.Add(_config.RamRammerShake);
            }
        }
    }
}
