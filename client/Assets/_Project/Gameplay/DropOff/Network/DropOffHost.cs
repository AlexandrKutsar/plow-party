using System;
using PlowParty.Gameplay.Bucket.Network;
using PlowParty.Gameplay.Vehicle.Network;
using VContainer.Unity;

namespace PlowParty.Gameplay.DropOff.Network
{
    public sealed class DropOffHost : IStartable, IDisposable
    {
        private readonly DeliveryRegistry _deliveries;
        private readonly BucketHost _buckets;

        public DropOffHost(DeliveryRegistry deliveries, BucketHost buckets)
        {
            _deliveries = deliveries;
            _buckets = buckets;
        }

        public void Start()
        {
            _buckets.Spilled += OnSpilled;
        }

        public void Dispose()
        {
            _buckets.Spilled -= OnSpilled;
        }

        private void OnSpilled(NetworkVehicle vehicle)
        {
            if (_deliveries.TryGet(vehicle, out var delivery))
            {
                delivery.Interrupt();
            }
        }
    }
}
