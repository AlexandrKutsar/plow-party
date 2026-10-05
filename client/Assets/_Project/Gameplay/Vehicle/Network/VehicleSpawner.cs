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

        [Inject]
        public void Construct(NetworkRunnerEvents events)
        {
            _events = events;
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
            if (!runner.IsServer)
            {
                return;
            }

            var spawnPoint = _spawnPoints[_spawned.Count % _spawnPoints.Length];
            _spawned[player] = runner.Spawn(_vehiclePrefab, spawnPoint.position, spawnPoint.rotation, player);
        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer || !_spawned.Remove(player, out var vehicle))
            {
                return;
            }

            runner.Despawn(vehicle);
        }
    }
}
