using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class VirtualStickView : MonoBehaviour
    {
        [SerializeField] private GameObject _stick;

        private IMatchClock _match;

        [Inject]
        public void Construct(IMatchClock match)
        {
            _match = match;
        }

        private void Update()
        {
            var visible = _match.IsRunning && _match.Phase == MatchPhase.Playing;
            if (_stick.activeSelf != visible)
            {
                _stick.SetActive(visible);
            }
        }
    }
}
