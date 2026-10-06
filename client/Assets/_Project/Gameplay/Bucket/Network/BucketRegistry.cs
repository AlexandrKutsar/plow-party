using System.Collections.Generic;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.Bucket.Network
{
    public sealed class BucketRegistry : IBladeRoom
    {
        private readonly Dictionary<NetworkVehicle, NetworkBucket> _buckets = new Dictionary<NetworkVehicle, NetworkBucket>();

        public void Add(NetworkVehicle vehicle, NetworkBucket bucket)
        {
            _buckets[vehicle] = bucket;
        }

        public void Remove(NetworkVehicle vehicle)
        {
            _buckets.Remove(vehicle);
        }

        public bool TryGet(NetworkVehicle vehicle, out NetworkBucket bucket)
        {
            return _buckets.TryGetValue(vehicle, out bucket);
        }

        public int RoomFor(NetworkVehicle vehicle)
        {
            return TryGet(vehicle, out var bucket) ? bucket.Room : 0;
        }
    }
}
