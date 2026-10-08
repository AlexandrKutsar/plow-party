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
        [SerializeField] private Button _menuButton;

        private IMatchClock _match;
        private IMatchResults _results;
        private VehicleRegistry _vehicles;
        private ParticipantRoster _roster;
        private IMatchExit _exit;

        [Inject]
        public void Construct(IMatchClock match, IMatchResults results, VehicleRegistry vehicles, ParticipantRoster roster, IMatchExit exit)
        {
            _match = match;
            _results = results;
            _vehicles = vehicles;
            _roster = roster;
            _exit = exit;
        }

        private void Awake()
        {
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            _menuButton.onClick.RemoveListener(OnMenuClicked);
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

        private void OnMenuClicked()
        {
            _exit.LeaveToMenu();
        }
    }
}
