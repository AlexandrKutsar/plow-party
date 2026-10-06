using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class ScorePopupView : MonoBehaviour
    {
        [SerializeField] private Text[] _popups;

        private VehicleRegistry _vehicles;
        private IScoreReader _scores;
        private Camera _camera;
        private HudConfig _config;
        private ScoreRise _rise;
        private Vector2[] _origins;
        private float[] _ages;
        private Color[] _colors;
        private NetworkVehicle _tracked;
        private int _next;

        [Inject]
        public void Construct(VehicleRegistry vehicles, IScoreReader scores, Camera worldCamera, HudConfig config)
        {
            _vehicles = vehicles;
            _scores = scores;
            _camera = worldCamera;
            _config = config;
            _rise = new ScoreRise(config.ScorePopupInterval);
        }

        private void Awake()
        {
            _origins = new Vector2[_popups.Length];
            _ages = new float[_popups.Length];
            _colors = new Color[_popups.Length];
            for (var i = 0; i < _popups.Length; i++)
            {
                _ages[i] = float.MaxValue;
                _colors[i] = _popups[i].color;
                _popups[i].gameObject.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            ObserveLocalScore(Time.deltaTime);
            Animate(Time.deltaTime);
        }

        private void ObserveLocalScore(float deltaTime)
        {
            if (!LocalVehicle.TryFind(_vehicles, out var vehicle))
            {
                _tracked = null;
                _rise.Forget();
                return;
            }

            if (vehicle != _tracked)
            {
                _tracked = vehicle;
                _rise.Forget();
            }

            var gained = _rise.Observe(_scores.ScoreOf(vehicle), deltaTime);
            if (gained > 0 && LocalVehicle.TryProjectAbove(_camera, vehicle, _config.LoadBarHeight, out var screenPoint))
            {
                Launch(gained, screenPoint);
            }
        }

        private void Launch(int gained, Vector2 origin)
        {
            var popup = _popups[_next];
            _origins[_next] = origin;
            _ages[_next] = 0f;
            popup.text = "+" + gained;
            popup.gameObject.SetActive(true);
            _next = (_next + 1) % _popups.Length;
        }

        private void Animate(float deltaTime)
        {
            for (var i = 0; i < _popups.Length; i++)
            {
                if (_ages[i] > _config.ScorePopupDuration)
                {
                    continue;
                }

                _ages[i] += deltaTime;
                var progress = Mathf.Clamp01(_ages[i] / _config.ScorePopupDuration);
                var popup = _popups[i];
                popup.rectTransform.position = _origins[i] + Vector2.up * (_config.ScorePopupRise * progress);
                var color = _colors[i];
                color.a *= 1f - progress * progress;
                popup.color = color;
                if (progress >= 1f)
                {
                    popup.gameObject.SetActive(false);
                }
            }
        }
    }
}
