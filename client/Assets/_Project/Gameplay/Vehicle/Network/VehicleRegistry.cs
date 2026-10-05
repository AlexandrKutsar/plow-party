using System;
using System.Collections.Generic;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class VehicleRegistry
    {
        private readonly List<NetworkVehicle> _vehicles = new List<NetworkVehicle>();

        public event Action<NetworkVehicle, NetworkVehicle, float> Rammed;

        public IReadOnlyList<NetworkVehicle> Vehicles => _vehicles;

        public void Add(NetworkVehicle vehicle)
        {
            _vehicles.Add(vehicle);
            _vehicles.Sort(static (left, right) => left.Object.Id.Raw.CompareTo(right.Object.Id.Raw));
        }

        public void Remove(NetworkVehicle vehicle)
        {
            _vehicles.Remove(vehicle);
        }

        public void ReportRam(NetworkVehicle rammer, NetworkVehicle victim, float strength)
        {
            Rammed?.Invoke(rammer, victim, strength);
        }
    }
}
