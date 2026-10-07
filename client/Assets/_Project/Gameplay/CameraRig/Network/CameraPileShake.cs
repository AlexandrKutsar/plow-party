using PlowParty.Gameplay.CameraRig.Config;
using PlowParty.Gameplay.CameraRig.Simulation;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Vehicle.Network;
using VContainer.Unity;

namespace PlowParty.Gameplay.CameraRig.Network
{
    public sealed class CameraPileShake : ITickable
    {
        private readonly VehicleRegistry _vehicles;
        private readonly SnowGridDriver _snow;
        private readonly ICameraShake _shake;
        private readonly CameraConfig _config;

        public CameraPileShake(VehicleRegistry vehicles, SnowGridDriver snow, ICameraShake shake, CameraConfig config)
        {
            _vehicles = vehicles;
            _snow = snow;
            _shake = shake;
            _config = config;
        }

        public void Tick()
        {
            if (_vehicles.TryGetLocal(out var local) && _snow.IsPlowingPile(local))
            {
                _shake.Sustain(_config.PilePlowShake);
            }
        }
    }
}
