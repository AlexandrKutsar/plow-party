using System.Collections.Generic;
using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.DropOff.Network
{
    public sealed class DeliveryRegistry : IScoreboard
    {
        private readonly Dictionary<NetworkVehicle, NetworkDelivery> _deliveries = new Dictionary<NetworkVehicle, NetworkDelivery>();

        public void Add(NetworkVehicle vehicle, NetworkDelivery delivery)
        {
            _deliveries[vehicle] = delivery;
        }

        public void Remove(NetworkVehicle vehicle)
        {
            _deliveries.Remove(vehicle);
        }

        public bool TryGet(NetworkVehicle vehicle, out NetworkDelivery delivery)
        {
            return _deliveries.TryGetValue(vehicle, out delivery);
        }

        public int ScoreOf(NetworkVehicle vehicle)
        {
            return TryGet(vehicle, out var delivery) ? delivery.Score : 0;
        }
    }
}
