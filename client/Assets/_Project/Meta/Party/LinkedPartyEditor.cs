using PlowParty.Meta.Party.Network;
using PlowParty.Shared;

namespace PlowParty.Meta.Party
{
    internal sealed class LinkedPartyEditor : IPartyEditor
    {
        private readonly PartyLinks _links;

        public LinkedPartyEditor(PartyLinks links)
        {
            _links = links;
        }

        public void SetMode(PartyMode mode)
        {
            _links.Current?.RequestMode(mode);
        }

        public void SetBot(int slot, BotDifficulty difficulty)
        {
            _links.Current?.RequestBot(slot, difficulty);
        }

        public void RemoveBot(int slot)
        {
            _links.Current?.RequestRemoveBot(slot);
        }

        public void PickColor(int color)
        {
            _links.Current?.RequestColor(color);
        }

        public void PickCritter(int critter)
        {
            _links.Current?.RequestCritter(critter);
        }
    }
}
