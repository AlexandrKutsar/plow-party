using UnityEngine;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class PodiumSpot
    {
        private const string SeatName = "CritterSeat";

        private readonly GameObject _vehicle;
        private readonly GameObject[] _critterModels;
        private readonly Transform _seat;
        private GameObject _critter;
        private GameObject _critterModel;

        public PodiumSpot(GameObject vehicle, GameObject[] critterModels)
        {
            _vehicle = vehicle;
            _critterModels = critterModels;
            _seat = FindSeat(vehicle.transform) ?? vehicle.transform;
            _vehicle.SetActive(false);
        }

        public void Show(MemberLook look, float offset)
        {
            _vehicle.transform.localPosition = Vector3.right * offset;
            _vehicle.SetActive(true);
            SeatCritter(ModelOf(look.Critter));
        }

        public void Hide()
        {
            _vehicle.SetActive(false);
        }

        private void SeatCritter(GameObject critterModel)
        {
            if (_critterModel == critterModel)
            {
                return;
            }

            if (_critter != null)
            {
                Object.Destroy(_critter);
            }

            _critterModel = critterModel;
            _critter = Object.Instantiate(critterModel, _seat, false);
        }

        private GameObject ModelOf(int critter)
        {
            var count = _critterModels.Length;
            return _critterModels[((critter % count) + count) % count];
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
