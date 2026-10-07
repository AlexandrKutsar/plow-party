using UnityEngine;

namespace PlowParty.Meta.Session.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Matchmaking Config", fileName = nameof(MatchmakingConfig))]
    public sealed class MatchmakingConfig : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _searchSeconds = 10f;
        [SerializeField, Range(1, 6)] private int _maxSlots = 6;
        [SerializeField, Min(1)] private int _roomCodeAttempts = 3;

        public float SearchSeconds => _searchSeconds;

        public int MaxSlots => _maxSlots;

        public int RoomCodeAttempts => _roomCodeAttempts;
    }
}
