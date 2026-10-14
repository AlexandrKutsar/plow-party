using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Participants.Config;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class NicknameLabelsView : MonoBehaviour
    {
        [SerializeField] private Text[] _labels;

        private VehicleRegistry _vehicles;
        private IMatchClock _match;
        private ParticipantRoster _roster;
        private ParticipantsConfig _participants;
        private Camera _camera;
        private HudConfig _config;
        private string[] _shownNicknames;
        private int[] _shownColors;

        [Inject]
        public void Construct(
            VehicleRegistry vehicles,
            IMatchClock match,
            ParticipantRoster roster,
            ParticipantsConfig participants,
            Camera worldCamera,
            HudConfig config)
        {
            _vehicles = vehicles;
            _match = match;
            _roster = roster;
            _participants = participants;
            _camera = worldCamera;
            _config = config;
        }

        private void Awake()
        {
            _shownNicknames = new string[_labels.Length];
            _shownColors = new int[_labels.Length];
            for (var i = 0; i < _shownColors.Length; i++)
            {
                _shownColors[i] = -1;
            }
        }

        private void LateUpdate()
        {
            var visible = _match.IsRunning && _match.Phase != MatchPhase.Results && _roster.IsReady;
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < _labels.Length; i++)
            {
                if (visible && i < vehicles.Count && _roster.IsSeated(vehicles[i].Slot)
                    && LocalVehicle.TryProjectAbove(_camera, vehicles[i], _config.NicknameLabelHeight, out var screenPoint))
                {
                    Show(i, vehicles[i].Slot, screenPoint);
                }
                else
                {
                    _labels[i].gameObject.SetActive(false);
                }
            }
        }

        private void Show(int index, int slot, Vector2 screenPoint)
        {
            var label = _labels[index];
            label.gameObject.SetActive(true);
            label.rectTransform.position = screenPoint;

            var nickname = _roster.NicknameOf(slot);
            if (!ReferenceEquals(nickname, _shownNicknames[index]))
            {
                _shownNicknames[index] = nickname;
                label.text = nickname;
            }

            var color = _roster.ColorOf(slot);
            if (color != _shownColors[index])
            {
                _shownColors[index] = color;
                label.color = _participants.ParticipantColor(color);
            }
        }
    }
}
