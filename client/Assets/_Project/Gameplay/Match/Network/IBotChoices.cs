using PlowParty.Shared;

namespace PlowParty.Gameplay.Match.Network
{
    public interface IBotChoices
    {
        bool TryGetChosenDifficulty(int slot, out BotDifficulty difficulty);
    }
}
