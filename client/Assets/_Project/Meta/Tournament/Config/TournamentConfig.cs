using Fusion;
using UnityEngine;

namespace PlowParty.Meta.Tournament.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Tournament Config", fileName = nameof(TournamentConfig))]
    public sealed class TournamentConfig : ScriptableObject
    {
        [SerializeField, Range(1, 50)] private int _topCount = 5;
        [SerializeField, Range(0, 10)] private int _aroundMeRadius = 2;
        [SerializeField] private NetworkObject _reportLinkPrefab;

        public int TopCount => _topCount;

        public int AroundMeRadius => _aroundMeRadius;

        public NetworkObject ReportLinkPrefab => _reportLinkPrefab;
    }
}
