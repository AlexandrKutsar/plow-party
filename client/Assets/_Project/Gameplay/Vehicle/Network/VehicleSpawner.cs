using System.Collections.Generic;
using Fusion;
using PlowParty.Infrastructure.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class VehicleSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject _vehiclePrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private readonly Dictionary<PlayerRef, NetworkObject> _spawned = new Dictionary<PlayerRef, NetworkObject>();
        private NetworkRunnerEvents _events;
        private VehicleRegistry _registry;
        private VehicleWorldDriver _driver;

        [Inject]
        public void Construct(NetworkRunnerEvents events, VehicleRegistry registry, VehicleWorldDriver driver)
        {
            _events = events;
            _registry = registry;
            _driver = driver;
            _events.PlayerJoined += OnPlayerJoined;
            _events.PlayerLeft += OnPlayerLeft;
        }

        private void OnDestroy()
        {
            if (_events == null)
            {
                return;
            }

            _events.PlayerJoined -= OnPlayerJoined;
            _events.PlayerLeft -= OnPlayerLeft;
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer || !TryTakeFreeSlot(out var slot))
            {
                return;
            }

            var spawnPoint = _spawnPoints[slot];
            _driver.ResetRamCooldowns(slot);
            _spawned[player] = runner.Spawn(
                _vehiclePrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                player,
                (_, spawned) => spawned.GetComponent<NetworkVehicle>().Slot = slot);
        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer || !_spawned.Remove(player, out var vehicle))
            {
                return;
            }

            runner.Despawn(vehicle);
        }

        private bool TryTakeFreeSlot(out int slot)
        {
            var slotCount = Mathf.Min(_spawnPoints.Length, VehicleWorldDriver.MaxVehicles);
            for (slot = 0; slot < slotCount; slot++)
            {
                if (!_registry.IsSlotTaken(slot))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
