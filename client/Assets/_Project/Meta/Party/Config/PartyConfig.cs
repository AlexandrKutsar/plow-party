using Fusion;
using UnityEngine;

namespace PlowParty.Meta.Party.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Party Config", fileName = nameof(PartyConfig))]
    public sealed class PartyConfig : ScriptableObject
    {
        [SerializeField, Range(1, 6)] private int _capacity = 6;
        [SerializeField, Min(1)] private int _codeAttempts = 3;
        [SerializeField, Min(1f)] private float _rejoinSeconds = 10f;
        [SerializeField, Min(1f)] private float _giveUpSeconds = 20f;
        [SerializeField, Min(0.1f)] private float _retryIntervalSeconds = 1f;
        [SerializeField, Min(0.1f)] private float _removalGraceSeconds = 1f;
        [SerializeField] private NetworkObject _partyLinkPrefab;

        public int Capacity => _capacity;

        public int CodeAttempts => _codeAttempts;

        public float RejoinSeconds => _rejoinSeconds;

        public float GiveUpSeconds => Mathf.Max(_rejoinSeconds, _giveUpSeconds);

        public float RetryIntervalSeconds => _retryIntervalSeconds;

        public float RemovalGraceSeconds => _removalGraceSeconds;

        public NetworkObject PartyLinkPrefab => _partyLinkPrefab;
    }
}
