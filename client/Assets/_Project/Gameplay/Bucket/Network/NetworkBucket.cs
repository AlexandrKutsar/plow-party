using Fusion;
using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.Bucket.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Bucket.Network
{
    [RequireComponent(typeof(NetworkVehicle))]
    public sealed class NetworkBucket : NetworkBehaviour
    {
        private BucketConfig _config;
        private BucketRegistry _registry;
        private BucketRules _rules;
        private NetworkVehicle _vehicle;

        [Networked] public int LoadSteps { get; private set; }

        public int Load => _rules.LoadUnits(LoadSteps);

        public bool IsFull => _rules.IsFull(LoadSteps);

        public int FreeSteps => _rules.FreeStepsFor(LoadSteps);

        [Inject]
        public void Construct(BucketConfig config, BucketRegistry registry)
        {
            _config = config;
            _registry = registry;
        }

        private void Awake()
        {
            _vehicle = GetComponent<NetworkVehicle>();
        }

        public override void Spawned()
        {
            _rules = new BucketRules(_config.ToSettings());
            _registry.Add(_vehicle, this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Remove(_vehicle);
        }

        public void Collect(int steps)
        {
            if (!HasStateAuthority)
            {
                return;
            }

            SetLoad(_rules.Collect(LoadSteps, steps));
        }

        public int Unload(int steps)
        {
            if (!HasStateAuthority)
            {
                return 0;
            }

            var unloaded = _rules.UnloadSteps(LoadSteps, steps);
            SetLoad(LoadSteps - unloaded);
            return unloaded;
        }

        public int TakeSpill()
        {
            if (!HasStateAuthority)
            {
                return 0;
            }

            var steps = _rules.SpillSteps(LoadSteps);
            SetLoad(LoadSteps - steps);
            return steps;
        }

        private void SetLoad(int loadSteps)
        {
            LoadSteps = loadSteps;
            _vehicle.SpeedMultiplier = _rules.SpeedMultiplier(loadSteps);
        }
    }
}
