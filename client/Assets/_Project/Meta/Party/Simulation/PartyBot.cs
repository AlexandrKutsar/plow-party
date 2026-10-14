using PlowParty.Shared;

namespace PlowParty.Meta.Party.Simulation
{
    public readonly struct PartyBot
    {
        public PartyBot(int slot, BotDifficulty difficulty)
        {
            Slot = slot;
            Difficulty = difficulty;
        }

        public int Slot { get; }

        public BotDifficulty Difficulty { get; }
    }
}
