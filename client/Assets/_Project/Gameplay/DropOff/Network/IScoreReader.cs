using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.DropOff.Network
{
    public interface IScoreReader
    {
        int ScoreOf(NetworkVehicle vehicle);

        float MultiplierOf(NetworkVehicle vehicle);

        bool IsDelivering(NetworkVehicle vehicle);
    }
}
