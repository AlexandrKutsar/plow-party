using System.Collections.Generic;
using PlowParty.Gameplay.Participants.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Participants.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Participants Config", fileName = nameof(ParticipantsConfig))]
    public sealed class ParticipantsConfig : ScriptableObject
    {
        [SerializeField] private string[] _botNicknames =
        {
            "Snowdrop", "Mittens", "Blizzy", "Frostbite", "Pinecone", "Sleet", "Icicle", "Flurry",
            "Tundra", "Avalanche", "Biscuit", "Maple", "Juniper", "Pebble", "Toboggan", "Cocoa",
            "Waffles", "Nugget", "Sprout", "Clover", "Hazel", "Pippin", "Muffin", "Noodle",
        };

        [SerializeField] private GameObject[] _critterModels = new GameObject[ParticipantProfiles.SpeciesCount];

        [SerializeField] private Color[] _colors =
        {
            new Color(0.835f, 0.369f, 0f),
            new Color(0.902f, 0.624f, 0f),
            new Color(0.898f, 0.78f, 0f),
            new Color(0f, 0.62f, 0.451f),
            new Color(0.337f, 0.706f, 0.914f),
            new Color(0f, 0.447f, 0.698f),
            new Color(0.8f, 0.475f, 0.655f),
            new Color(0.235f, 0.235f, 0.275f),
        };

        public IReadOnlyList<string> BotNicknames => _botNicknames;

        public int ColorCount => _colors.Length;

        public Color ParticipantColor(int color)
        {
            return color >= 0 && color < _colors.Length ? _colors[color] : Color.white;
        }

        public GameObject CritterModel(CritterSpecies species)
        {
            var index = (int)species;
            return index >= 0 && index < _critterModels.Length ? _critterModels[index] : null;
        }
    }
}
