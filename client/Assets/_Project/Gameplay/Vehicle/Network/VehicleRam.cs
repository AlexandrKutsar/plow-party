namespace PlowParty.Gameplay.Vehicle.Network
{
    public readonly struct VehicleRam
    {
        public VehicleRam(NetworkVehicle rammer, NetworkVehicle victim, float strength)
        {
            Rammer = rammer;
            Victim = victim;
            Strength = strength;
        }

        public NetworkVehicle Rammer { get; }

        public NetworkVehicle Victim { get; }

        public float Strength { get; }
    }
}
