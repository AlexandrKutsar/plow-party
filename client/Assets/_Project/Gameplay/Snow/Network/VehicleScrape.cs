using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.Snow.Network
{
    public readonly struct VehicleScrape
    {
        public VehicleScrape(NetworkVehicle vehicle, int steps)
        {
            Vehicle = vehicle;
            Steps = steps;
        }

        public NetworkVehicle Vehicle { get; }

        public int Steps { get; }
    }
}
