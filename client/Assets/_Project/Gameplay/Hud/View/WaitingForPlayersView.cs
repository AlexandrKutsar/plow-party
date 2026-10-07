using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Participants.Network;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class WaitingForPlayersView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Text _timerLabel;

        private IMatchClock _match;
        private ParticipantRoster _roster;
        private int _shownSeated = -1;
        private int _shownSlots = -1;
        private int _shownSeconds = -1;

        [Inject]
        public void Construct(IMatchClock match, ParticipantRoster roster)
        {
            _match = match;
            _roster = roster;
        }

        private void LateUpdate()
        {
            var visible = _match.IsRunning && _match.Phase == MatchPhase.WaitingForPlayers && _roster.IsReady;
            if (_panel.activeSelf != visible)
            {
                _panel.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            ShowStatus(_roster.SeatedCount, _roster.SlotCount);
            ShowTimer(HudText.WholeSecondsLeft(_match.PhaseRemaining));
        }

        private void ShowStatus(int seated, int slots)
        {
            if (seated == _shownSeated && slots == _shownSlots)
            {
                return;
            }

            _shownSeated = seated;
            _shownSlots = slots;
            _statusLabel.text = HudText.WaitingForPlayers(seated, slots);
        }

        private void ShowTimer(int seconds)
        {
            if (seconds == _shownSeconds)
            {
                return;
            }

            _shownSeconds = seconds;
            _timerLabel.text = HudText.Clock(seconds);
        }
    }
}
