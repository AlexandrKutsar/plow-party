using PlowParty.Gameplay.Match.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Match.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Match Config", fileName = nameof(MatchConfig))]
    public sealed class MatchConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _countdownDuration = 3f;
        [SerializeField, Min(1f)] private float _playingDuration = 180f;
        [SerializeField, Min(0f)] private float _resultsDuration = 10f;

        public MatchSettings ToSettings()
        {
            return new MatchSettings
            {
                CountdownDuration = _countdownDuration,
                PlayingDuration = _playingDuration,
                ResultsDuration = _resultsDuration,
            };
        }
    }
}
