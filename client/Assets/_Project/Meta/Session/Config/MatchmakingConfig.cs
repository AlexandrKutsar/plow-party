using UnityEngine;

namespace PlowParty.Meta.Session.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Matchmaking Config", fileName = nameof(MatchmakingConfig))]
    public sealed class MatchmakingConfig : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _searchSeconds = 10f;
        [SerializeField, Range(1, 6)] private int _maxSlots = 6;
        [SerializeField, Min(0f)] private float _hostJitterMinSeconds = 0.3f;
        [SerializeField, Min(0f)] private float _hostJitterMaxSeconds = 1.5f;

        public float SearchSeconds => _searchSeconds;

        public int MaxSlots => _maxSlots;

        public float HostJitterMinSeconds => _hostJitterMinSeconds;

        public float HostJitterMaxSeconds => Mathf.Max(_hostJitterMinSeconds, _hostJitterMaxSeconds);
    }
}
