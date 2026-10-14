using PlowParty.Gameplay.Participants.Config;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Participants.View
{
    [RequireComponent(typeof(NetworkVehicle))]
    public sealed class VehicleBodyColorView : MonoBehaviour
    {
        private const string BodyName = "Body";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private ParticipantRoster _roster;
        private ParticipantsConfig _config;
        private NetworkVehicle _vehicle;
        private Renderer _body;
        private MaterialPropertyBlock _properties;
        private int _shownColor = -1;

        [Inject]
        public void Construct(ParticipantRoster roster, ParticipantsConfig config)
        {
            _roster = roster;
            _config = config;
        }

        private void Awake()
        {
            _vehicle = GetComponent<NetworkVehicle>();
            _body = FindBody(transform);
            _properties = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            if (_body == null || _vehicle.Object == null || !_vehicle.Object.IsValid)
            {
                return;
            }

            if (_roster.TryGetProfile(_vehicle.Slot, out var profile) && profile.Color != _shownColor)
            {
                Paint(profile.Color);
            }
        }

        private void Paint(int color)
        {
            _shownColor = color;
            _body.GetPropertyBlock(_properties);
            _properties.SetColor(BaseColorId, _config.ParticipantColor(color));
            _body.SetPropertyBlock(_properties);
        }

        private static Renderer FindBody(Transform root)
        {
            if (root.name == BodyName && root.TryGetComponent<Renderer>(out var body))
            {
                return body;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindBody(root.GetChild(i));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
