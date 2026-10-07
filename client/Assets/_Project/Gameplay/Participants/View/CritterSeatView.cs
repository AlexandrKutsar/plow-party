using PlowParty.Gameplay.Participants.Config;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Participants.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Participants.View
{
    [RequireComponent(typeof(NetworkVehicle))]
    public sealed class CritterSeatView : MonoBehaviour
    {
        private const string SeatName = "CritterSeat";

        private ParticipantRoster _roster;
        private ParticipantsConfig _config;
        private NetworkVehicle _vehicle;
        private Transform _seat;
        private GameObject _critter;
        private CritterSpecies _shownSpecies;
        private bool _isSeated;

        [Inject]
        public void Construct(ParticipantRoster roster, ParticipantsConfig config)
        {
            _roster = roster;
            _config = config;
        }

        private void Awake()
        {
            _vehicle = GetComponent<NetworkVehicle>();
            _seat = FindSeat(transform);
        }

        private void LateUpdate()
        {
            if (_seat == null || _vehicle.Object == null || !_vehicle.Object.IsValid)
            {
                return;
            }

            if (!_roster.TryGetProfile(_vehicle.Slot, out var profile))
            {
                return;
            }

            if (!_isSeated || profile.Species != _shownSpecies)
            {
                Seat(profile.Species);
            }
        }

        private void Seat(CritterSpecies species)
        {
            if (_critter != null)
            {
                Destroy(_critter);
            }

            var model = _config.CritterModel(species);
            _shownSpecies = species;
            _isSeated = true;
            _critter = model != null ? Instantiate(model, _seat, false) : null;
        }

        private static Transform FindSeat(Transform root)
        {
            if (root.name == SeatName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var seat = FindSeat(root.GetChild(i));
                if (seat != null)
                {
                    return seat;
                }
            }

            return null;
        }
    }
}
