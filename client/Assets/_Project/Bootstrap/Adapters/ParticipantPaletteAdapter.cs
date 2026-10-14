using PlowParty.Gameplay.Participants.Config;
using PlowParty.Gameplay.Participants.Simulation;
using PlowParty.Meta.Party;
using UnityEngine;

namespace PlowParty.Bootstrap.Adapters
{
    public sealed class ParticipantPaletteAdapter : IParticipantPalette
    {
        private readonly ParticipantsConfig _config;

        public ParticipantPaletteAdapter(ParticipantsConfig config)
        {
            _config = config;
        }

        public int ColorCount => _config.ColorCount;

        public int CritterCount => ParticipantProfiles.SpeciesCount;

        public Color ColorOf(int color)
        {
            return _config.ParticipantColor(color);
        }
    }
}
