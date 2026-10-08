using PlowParty.Meta.Party;
using PlowParty.Meta.Session;

namespace PlowParty.Meta.Lobby.Simulation
{
    public static class LobbyScreens
    {
        public static LobbyScreen For(PartyStage party, LobbyStage search)
        {
            if (search != LobbyStage.Idle)
            {
                return LobbyScreen.Search;
            }

            return party == PartyStage.None ? LobbyScreen.Solo : LobbyScreen.Party;
        }
    }
}
