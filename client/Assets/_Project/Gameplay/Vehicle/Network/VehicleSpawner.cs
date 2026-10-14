using System.Collections.Generic;
using Fusion;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class VehicleSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject _vehiclePrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private readonly Dictionary<int, NetworkObject> _spawned = new Dictionary<int, NetworkObject>();
        private VehicleWorldDriver _driver;

        public int SlotCount => Mathf.Min(_spawnPoints.Length, VehicleWorldDriver.MaxVehicles);

        [Inject]
        public void Construct(VehicleWorldDriver driver)
        {
            _driver = driver;
        }

        public bool IsSlotTaken(int slot)
        {
            return _spawned.ContainsKey(slot);
        }

        public void Spawn(NetworkRunner runner, int slot, PlayerRef driver)
        {
            if (!runner.IsServer || slot < 0 || slot >= SlotCount || IsSlotTaken(slot))
            {
                return;
            }

            var spawnPoint = _spawnPoints[slot];
            _driver.ResetRamCooldowns(slot);
            _spawned[slot] = runner.Spawn(
                _vehiclePrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                driver,
                (_, spawned) => spawned.GetComponent<NetworkVehicle>().Slot = slot);
        }

        public void Despawn(NetworkRunner runner, int slot)
        {
            if (!runner.IsServer || !_spawned.Remove(slot, out var vehicle))
            {
                return;
            }

            runner.Despawn(vehicle);
        }
    }
}
