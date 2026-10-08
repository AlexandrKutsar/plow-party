using Fusion;
using UnityEngine;

namespace PlowParty.Meta.Session.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Matchmaking Config", fileName = nameof(MatchmakingConfig))]
    public sealed class MatchmakingConfig : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _searchSeconds = 10f;
        [SerializeField, Range(1, 6)] private int _maxSlots = 6;
        [SerializeField, Min(0f)] private float _minSecondsBeforeStart = 3f;
        [SerializeField, Min(0.5f)] private float _listWaitSeconds = 3f;
        [SerializeField, Min(0.1f)] private float _mergeCheckSeconds = 1f;
        [SerializeField, Min(0f)] private float _moveGraceSeconds = 0.3f;
        [SerializeField] private NetworkObject _lobbyLinkPrefab;

        public float SearchSeconds => _searchSeconds;

        public int MaxSlots => _maxSlots;

        public float MinSecondsBeforeStart => _minSecondsBeforeStart;

        public float ListWaitSeconds => _listWaitSeconds;

        public float MergeCheckSeconds => _mergeCheckSeconds;

        public float MoveGraceSeconds => _moveGraceSeconds;

        public NetworkObject LobbyLinkPrefab => _lobbyLinkPrefab;
    }
}
