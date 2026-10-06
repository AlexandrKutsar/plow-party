using Fusion;
using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.Bucket.Network;
using PlowParty.Gameplay.DropOff.Config;
using PlowParty.Gameplay.DropOff.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.DropOff.Network
{
    [RequireComponent(typeof(NetworkVehicle), typeof(NetworkBucket))]
    public sealed class NetworkDelivery : NetworkBehaviour
    {
        private DropOffConfig _config;
        private BucketConfig _bucketConfig;
        private DropOffZone _zone;
        private DeliveryRegistry _registry;
        private DropOffRules _rules;
        private NetworkVehicle _vehicle;
        private NetworkBucket _bucket;

        [Networked] public int Score { get; private set; }

        [Networked] public float Multiplier { get; private set; }

        [Networked] private int DeliveredSteps { get; set; }

        [Networked] private float DeliveryElapsed { get; set; }

        public bool IsDelivering => Multiplier > 0f;

        [Inject]
        public void Construct(DropOffConfig config, BucketConfig bucketConfig, DropOffZone zone, DeliveryRegistry registry)
        {
            _config = config;
            _bucketConfig = bucketConfig;
            _zone = zone;
            _registry = registry;
        }

        private void Awake()
        {
            _vehicle = GetComponent<NetworkVehicle>();
            _bucket = GetComponent<NetworkBucket>();
        }

        public override void Spawned()
        {
            _rules = new DropOffRules(_config.ToSettings(), _bucketConfig.ToSettings());
            _registry.Add(_vehicle, this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Remove(_vehicle);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            var inZone = _rules.IsInZone(_zone.Centre, _vehicle.Position);
            var tick = _rules.Tick(ReadDelivery(), inZone, _bucket.LoadSteps, Runner.DeltaTime);
            if (tick.UnloadedSteps > 0)
            {
                _bucket.Unload(tick.UnloadedSteps);
            }

            Score += tick.ScoreGained;
            WriteDelivery(tick.Next);
        }

        public void Interrupt()
        {
            if (HasStateAuthority)
            {
                WriteDelivery(Delivery.None);
            }
        }

        private Delivery ReadDelivery()
        {
            return new Delivery(Multiplier, DeliveredSteps, DeliveryElapsed);
        }

        private void WriteDelivery(Delivery delivery)
        {
            Multiplier = delivery.Multiplier;
            DeliveredSteps = delivery.DeliveredSteps;
            DeliveryElapsed = delivery.Elapsed;
        }
    }
}
