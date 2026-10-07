using System;
using System.Collections.Generic;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class VehicleRegistry
    {
        private readonly List<NetworkVehicle> _vehicles = new List<NetworkVehicle>();

        public event Action<VehicleRam> Rammed;

        public IReadOnlyList<NetworkVehicle> Vehicles => _vehicles;

        public void Add(NetworkVehicle vehicle)
        {
            _vehicles.Add(vehicle);
            _vehicles.Sort(static (left, right) => left.Slot.CompareTo(right.Slot));
        }

        public void Remove(NetworkVehicle vehicle)
        {
            _vehicles.Remove(vehicle);
        }

        public bool TryGetLocal(out NetworkVehicle local)
        {
            for (var i = 0; i < _vehicles.Count; i++)
            {
                if (_vehicles[i].HasInputAuthority)
                {
                    local = _vehicles[i];
                    return true;
                }
            }

            local = null;
            return false;
        }

        public void ReportRam(VehicleRam ram)
        {
            Rammed?.Invoke(ram);
        }
    }
}
