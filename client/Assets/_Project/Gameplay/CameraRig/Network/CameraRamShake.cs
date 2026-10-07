using PlowParty.Gameplay.CameraRig.Config;
using PlowParty.Gameplay.CameraRig.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Gameplay.Vehicle.Simulation;
using VContainer.Unity;

namespace PlowParty.Gameplay.CameraRig.Network
{
    public sealed class CameraRamShake : ITickable
    {
        private readonly VehicleRegistry _vehicles;
        private readonly ICameraShake _shake;
        private readonly CameraConfig _config;
        private readonly RamWatch _watch = new RamWatch();
        private NetworkVehicle _watched;

        public CameraRamShake(VehicleRegistry vehicles, ICameraShake shake, CameraConfig config)
        {
            _vehicles = vehicles;
            _shake = shake;
            _config = config;
        }

        public void Tick()
        {
            if (!_vehicles.TryGetLocal(out var local))
            {
                _watched = null;
                return;
            }

            if (local != _watched)
            {
                _watched = local;
                _watch.Reset();
            }

            var role = _watch.Observe(local.TimesRammed, local.RamsDealt);
            if (role == RamRole.Victim)
            {
                _shake.Add(_config.RamVictimShake);
            }
            else if (role == RamRole.Rammer)
            {
                _shake.Add(_config.RamRammerShake);
            }
        }
    }
}
