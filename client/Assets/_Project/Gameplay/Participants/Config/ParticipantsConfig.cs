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

        public IReadOnlyList<string> BotNicknames => _botNicknames;

        public GameObject CritterModel(CritterSpecies species)
        {
            var index = (int)species;
            return index >= 0 && index < _critterModels.Length ? _critterModels[index] : null;
        }
    }
}
