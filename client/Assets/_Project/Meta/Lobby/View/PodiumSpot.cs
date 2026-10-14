using UnityEngine;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class PodiumSpot
    {
        private const string SeatName = "CritterSeat";
        private const string BodyName = "Body";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly GameObject _vehicle;
        private readonly GameObject[] _critterModels;
        private readonly Transform _seat;
        private readonly Renderer _body;
        private readonly MaterialPropertyBlock _properties = new MaterialPropertyBlock();
        private GameObject _critter;
        private GameObject _critterModel;
        private Color? _shownColor;

        public PodiumSpot(GameObject vehicle, GameObject[] critterModels)
        {
            _vehicle = vehicle;
            _critterModels = critterModels;
            _seat = Find(vehicle.transform, SeatName) ?? vehicle.transform;
            var body = Find(vehicle.transform, BodyName);
            _body = body != null ? body.GetComponent<Renderer>() : null;
            _vehicle.SetActive(false);
        }

        public void Show(MemberLook look, float offset)
        {
            _vehicle.transform.localPosition = Vector3.right * offset;
            _vehicle.SetActive(true);
            Paint(look.Color);
            SeatCritter(ModelOf(look.Critter));
        }

        public void Hide()
        {
            _vehicle.SetActive(false);
        }

        private void Paint(Color color)
        {
            if (_body == null || _shownColor == color)
            {
                return;
            }

            _shownColor = color;
            _body.GetPropertyBlock(_properties);
            _properties.SetColor(BaseColorId, color);
            _body.SetPropertyBlock(_properties);
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

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = Find(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
