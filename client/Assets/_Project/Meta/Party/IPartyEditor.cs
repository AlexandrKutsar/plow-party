using PlowParty.Shared;

namespace PlowParty.Meta.Party
{
    internal interface IPartyEditor
    {
        void SetMode(PartyMode mode);

        void SetBot(int slot, BotDifficulty difficulty);

        void RemoveBot(int slot);

        void PickColor(int color);

        void PickCritter(int critter);
    }
}
