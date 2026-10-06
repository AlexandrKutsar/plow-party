using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.Snow.Network
{
    public interface IScrapeLimit
    {
        int LimitFor(NetworkVehicle vehicle);
    }
}
