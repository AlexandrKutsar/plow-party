using Fusion;
using PlowParty.Gameplay.Vehicle.Config;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class VehicleWorldDriver : NetworkBehaviour
    {
        private const int MaxVehicles = 6;
        private const int MaxPairs = MaxVehicles * (MaxVehicles - 1) / 2;

        [SerializeField] private Transform _arenaRoot;

        private VehicleRegistry _registry;
        private VehicleConfig _config;
        private VehicleWorld _world;

        [Networked, Capacity(MaxPairs)] public NetworkArray<float> RamCooldowns { get; }

        [Inject]
        public void Construct(VehicleRegistry registry, VehicleConfig config)
        {
            _registry = registry;
            _config = config;
        }

        public override void Spawned()
        {
            _world = new VehicleWorld(_config.ToSettings(), VehicleArenaReader.Read(_arenaRoot), MaxVehicles);
            Runner.SetIsSimulated(Object, true);
        }

        public override void FixedUpdateNetwork()
        {
            var vehicles = _registry.Vehicles;
            var count = Mathf.Min(vehicles.Count, MaxVehicles);
            _world.Clear();
            for (var i = 0; i < count; i++)
            {
                var vehicle = vehicles[i];
                _world.Add(vehicle.ReadState());
                if (Runner.TryGetInputForPlayer<VehicleNetworkInput>(vehicle.Object.InputAuthority, out var input))
                {
                    vehicle.LastMove = input.Move;
                }

                _world.SetControl(i, VehicleInput.Stick(vehicle.LastMove), VehicleModifiers.None);
            }

            LoadRamCooldowns(count);
            _world.Tick(Runner.DeltaTime);
            StoreRamCooldowns(count);

            for (var i = 0; i < count; i++)
            {
                vehicles[i].WriteState(_world.GetVehicle(i));
            }

            if (!Runner.IsForward)
            {
                return;
            }

            var rams = _world.Rams;
            for (var i = 0; i < rams.Count; i++)
            {
                _registry.ReportRam(vehicles[rams[i].Rammer], vehicles[rams[i].Victim], rams[i].Strength);
            }
        }

        private void LoadRamCooldowns(int count)
        {
            var pair = 0;
            for (var second = 1; second < count; second++)
            {
                for (var first = 0; first < second; first++)
                {
                    _world.SetRamCooldown(first, second, RamCooldowns[pair++]);
                }
            }
        }

        private void StoreRamCooldowns(int count)
        {
            var pair = 0;
            for (var second = 1; second < count; second++)
            {
                for (var first = 0; first < second; first++)
                {
                    RamCooldowns.Set(pair++, _world.GetRamCooldown(first, second));
                }
            }
        }
    }
}
