using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class ResultsView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private ScoreRowView[] _rows;
        [SerializeField] private Text _nextMatchLabel;
        [SerializeField] private Button _restartButton;

        private IMatchClock _match;
        private IMatchResults _results;
        private VehicleRegistry _vehicles;
        private ParticipantRoster _roster;
        private int _shownSeconds = -1;

        [Inject]
        public void Construct(IMatchClock match, IMatchResults results, VehicleRegistry vehicles, ParticipantRoster roster)
        {
            _match = match;
            _results = results;
            _vehicles = vehicles;
            _roster = roster;
        }

        private void Awake()
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        private void LateUpdate()
        {
            var visible = _match.IsRunning && _match.Phase == MatchPhase.Results;
            _panel.SetActive(visible);
            if (!visible)
            {
                return;
            }

            ShowPlacements();
            ShowNextMatch(_results.WaitsForHost ? 0 : Mathf.Max(1, HudText.WholeSecondsLeft(_match.PhaseRemaining)));
            _restartButton.gameObject.SetActive(_results.CanRequestRestart);
        }

        private void ShowPlacements()
        {
            var localSlot = LocalVehicle.TryFind(_vehicles, out var local) ? local.Slot : -1;
            for (var i = 0; i < _rows.Length; i++)
            {
                if (i >= _results.PlacementCount)
                {
                    _rows[i].Hide();
                    continue;
                }

                var placement = _results.GetPlacement(i);
                _rows[i].Show(_roster.NicknameOf(placement.Slot), placement.Slot == localSlot, placement.Score);
                _rows[i].ShowPlace(placement.Place);
            }
        }

        private void ShowNextMatch(int seconds)
        {
            if (seconds == _shownSeconds)
            {
                return;
            }

            _shownSeconds = seconds;
            _nextMatchLabel.text = seconds > 0 ? HudText.NextMatchIn(seconds) : HudText.WaitingForHost;
        }

        private void OnRestartClicked()
        {
            _results.RequestRestart();
        }
    }
}
