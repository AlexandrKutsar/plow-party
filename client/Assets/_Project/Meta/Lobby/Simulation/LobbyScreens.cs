using PlowParty.Meta.Party;
using PlowParty.Meta.Session;
using PlowParty.Shared;

namespace PlowParty.Meta.Lobby.Simulation
{
    public static class LobbyScreens
    {
        public static LobbyScreen For(PartyStage party, LobbyStage search, PartyMode soloMode = PartyMode.QuickPlay)
        {
            if (search != LobbyStage.Idle)
            {
                return LobbyScreen.Search;
            }

            if (party != PartyStage.None)
            {
                return LobbyScreen.Party;
            }

            return soloMode == PartyMode.CustomGame ? LobbyScreen.Party : LobbyScreen.Solo;
        }
    }
}
