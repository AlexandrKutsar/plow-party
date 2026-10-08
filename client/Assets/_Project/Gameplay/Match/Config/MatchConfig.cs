using PlowParty.Gameplay.Match.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Match.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Match Config", fileName = nameof(MatchConfig))]
    public sealed class MatchConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _waitingDuration = 15f;
        [SerializeField, Min(0f)] private float _botArrivalStart = 1f;
        [SerializeField, Min(0f)] private float _botArrivalEnd = 8f;
        [SerializeField, Min(0f)] private float _quickBotArrivalStart = 0.5f;
        [SerializeField, Min(0f)] private float _quickBotArrivalEnd = 2.5f;
        [SerializeField, Range(1, 6)] private int _fallbackSlotCount = 6;
        [SerializeField, Min(0f)] private float _countdownDuration = 3f;
        [SerializeField, Min(1f)] private float _playingDuration = 180f;

        public MatchSettings ToSettings()
        {
            return new MatchSettings
            {
                WaitingDuration = _waitingDuration,
                BotArrivalStart = _botArrivalStart,
                BotArrivalEnd = Mathf.Max(_botArrivalStart, _botArrivalEnd),
                QuickBotArrivalStart = _quickBotArrivalStart,
                QuickBotArrivalEnd = Mathf.Max(_quickBotArrivalStart, _quickBotArrivalEnd),
                FallbackSlotCount = _fallbackSlotCount,
                CountdownDuration = _countdownDuration,
                PlayingDuration = _playingDuration,
            };
        }
    }
}
