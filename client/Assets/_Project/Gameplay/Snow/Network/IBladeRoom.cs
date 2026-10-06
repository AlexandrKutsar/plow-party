using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.Snow.Network
{
    public interface IBladeRoom
    {
        int RoomFor(NetworkVehicle vehicle);
    }
}
