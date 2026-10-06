using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.DropOff.Network
{
    public interface IScoreboard
    {
        int ScoreOf(NetworkVehicle vehicle);
    }
}
