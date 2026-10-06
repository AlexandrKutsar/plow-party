using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class DropOffArrowView : MonoBehaviour
    {
        private const float ArrowArtAngle = 90f;

        [SerializeField] private RectTransform _arrow;
        [SerializeField] private RectTransform _pointer;

        private DropOffZone _zone;
        private Camera _camera;
        private IMatchClock _match;
        private HudConfig _config;
        private Canvas _canvas;

        [Inject]
        public void Construct(DropOffZone zone, Camera worldCamera, IMatchClock match, HudConfig config)
        {
            _zone = zone;
            _camera = worldCamera;
            _match = match;
            _config = config;
        }

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        private void LateUpdate()
        {
            var screenPoint = _camera.WorldToScreenPoint(PlaneProjection.ToWorld(_zone.Centre, 0f));
            var margin = _config.DropOffArrowMargin * _canvas.scaleFactor;
            var screenSize = new Vector2(Screen.width, Screen.height);
            var position = Vector2.zero;
            var angle = 0f;
            var visible = _match.IsRunning
                && _match.Phase != MatchPhase.Results
                && EdgeArrow.TryPlace(screenPoint, screenSize, margin, out position, out angle);
            _arrow.gameObject.SetActive(visible);
            if (visible)
            {
                _arrow.position = position;
                _pointer.localRotation = Quaternion.Euler(0f, 0f, angle - ArrowArtAngle);
            }
        }
    }
}
