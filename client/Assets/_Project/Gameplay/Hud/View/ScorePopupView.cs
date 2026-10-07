using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
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
        private IMatchClock _match;
        private Camera _camera;
        private HudConfig _config;
        private ScorePopupBatch _batch;
        private Vector2[] _origins;
        private float[] _ages;
        private Color[] _colors;
        private NetworkVehicle _tracked;
        private int _next;

        [Inject]
        public void Construct(VehicleRegistry vehicles, IScoreReader scores, IMatchClock match, Camera worldCamera, HudConfig config)
        {
            _match = match;
            _vehicles = vehicles;
            _scores = scores;
            _camera = worldCamera;
            _config = config;
            _batch = new ScorePopupBatch(config.ScorePopupInterval);
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
            var playing = _match.IsRunning && _match.Phase == MatchPhase.Playing;
            if (!playing || !LocalVehicle.TryFind(_vehicles, out var vehicle))
            {
                _tracked = null;
                _batch.Forget();
                return;
            }

            if (vehicle != _tracked)
            {
                _tracked = vehicle;
                _batch.Forget();
            }

            var gained = _batch.Observe(_scores.ScoreOf(vehicle), deltaTime);
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
            popup.text = HudText.ScoreGain(gained);
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
