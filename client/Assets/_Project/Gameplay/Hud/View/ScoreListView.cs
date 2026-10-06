using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class ScoreListView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private ScoreRowView[] _rows;

        private VehicleRegistry _vehicles;
        private IScoreReader _scores;
        private IMatchClock _match;
        private NetworkVehicle[] _order;
        private int[] _orderScores;

        [Inject]
        public void Construct(VehicleRegistry vehicles, IScoreReader scores, IMatchClock match)
        {
            _vehicles = vehicles;
            _scores = scores;
            _match = match;
        }

        private void Awake()
        {
            _order = new NetworkVehicle[_rows.Length];
            _orderScores = new int[_rows.Length];
        }

        private void LateUpdate()
        {
            var visible = _match.IsRunning && _match.Phase != MatchPhase.Results;
            _panel.SetActive(visible);
            if (!visible)
            {
                return;
            }

            var count = SortByScore();
            for (var i = 0; i < _rows.Length; i++)
            {
                if (i < count)
                {
                    _rows[i].Show(_order[i].Slot, _order[i].HasInputAuthority, _orderScores[i]);
                }
                else
                {
                    _rows[i].Hide();
                }
            }
        }

        private int SortByScore()
        {
            var vehicles = _vehicles.Vehicles;
            var count = Mathf.Min(vehicles.Count, _order.Length);
            for (var i = 0; i < count; i++)
            {
                var vehicle = vehicles[i];
                var score = _scores.ScoreOf(vehicle);
                var index = i;
                while (index > 0 && _orderScores[index - 1] < score)
                {
                    _order[index] = _order[index - 1];
                    _orderScores[index] = _orderScores[index - 1];
                    index--;
                }

                _order[index] = vehicle;
                _orderScores[index] = score;
            }

            return count;
        }
    }
}
