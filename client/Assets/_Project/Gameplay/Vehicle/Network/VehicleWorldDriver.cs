using Fusion;
using PlowParty.Gameplay.Vehicle.Config;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class VehicleWorldDriver : NetworkBehaviour
    {
        public const int MaxVehicles = 6;

        [SerializeField] private Transform _arenaRoot;

        private VehicleRegistry _registry;
        private VehicleConfig _config;
        private VehicleWorld _world;

        [Networked, Capacity(MaxVehicles * (MaxVehicles - 1) / 2)] private NetworkArray<float> SlotRamCooldowns { get; }

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

        public void ResetRamCooldowns(int slot)
        {
            for (var other = 0; other < MaxVehicles; other++)
            {
                if (other != slot)
                {
                    SlotRamCooldowns.Set(VehiclePairs.Index(slot, other), 0f);
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            var vehicles = _registry.Vehicles;
            _world.Clear();
            for (var i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                _world.Add(vehicle.ReadState());
                _world.SetControl(i, vehicle.ReadInput(), vehicle.ConsumeModifiers());
            }

            CopySlotCooldownsToWorld(vehicles.Count);
            _world.Tick(Runner.DeltaTime);
            CopyWorldCooldownsToSlots(vehicles.Count);

            for (var i = 0; i < vehicles.Count; i++)
            {
                vehicles[i].WriteState(_world.GetVehicle(i));
            }

            if (Runner.IsServer)
            {
                ReportRams();
            }
        }

        private void ReportRams()
        {
            var vehicles = _registry.Vehicles;
            var rams = _world.Rams;
            for (var i = 0; i < rams.Count; i++)
            {
                var rammer = vehicles[rams[i].Rammer];
                var victim = vehicles[rams[i].Victim];
                rammer.CountRamAsRammer();
                victim.CountRamAsVictim();
                _registry.ReportRam(new VehicleRam(rammer, victim, rams[i].Strength));
            }
        }

        private void CopySlotCooldownsToWorld(int count)
        {
            var vehicles = _registry.Vehicles;
            for (var second = 1; second < count; second++)
            {
                for (var first = 0; first < second; first++)
                {
                    var slotPair = VehiclePairs.Index(vehicles[first].Slot, vehicles[second].Slot);
                    _world.SetRamCooldown(first, second, SlotRamCooldowns[slotPair]);
                }
            }
        }

        private void CopyWorldCooldownsToSlots(int count)
        {
            var vehicles = _registry.Vehicles;
            for (var second = 1; second < count; second++)
            {
                for (var first = 0; first < second; first++)
                {
                    var slotPair = VehiclePairs.Index(vehicles[first].Slot, vehicles[second].Slot);
                    SlotRamCooldowns.Set(slotPair, _world.GetRamCooldown(first, second));
                }
            }
        }
    }
}
