using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.Bucket.Network;
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
    public sealed class LoadBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform _anchor;
        [SerializeField] private Image _fill;
        [SerializeField] private Color _fillingColor = Color.white;
        [SerializeField] private Color _fullColor = Color.yellow;
        [SerializeField] private GameObject _multiplierBadge;
        [SerializeField] private Text _multiplierLabel;

        private VehicleRegistry _vehicles;
        private BucketRegistry _buckets;
        private IScoreReader _scores;
        private IMatchClock _match;
        private Camera _camera;
        private HudConfig _config;
        private float _capacity;
        private float _shownMultiplier = -1f;

        [Inject]
        public void Construct(
            VehicleRegistry vehicles,
            BucketRegistry buckets,
            IScoreReader scores,
            IMatchClock match,
            Camera worldCamera,
            HudConfig config,
            BucketConfig bucketConfig)
        {
            _vehicles = vehicles;
            _buckets = buckets;
            _scores = scores;
            _match = match;
            _camera = worldCamera;
            _config = config;
            _capacity = bucketConfig.ToSettings().Capacity;
        }

        private void LateUpdate()
        {
            if (!_match.IsRunning
                || _match.Phase == MatchPhase.Results
                || !LocalVehicle.TryFind(_vehicles, out var vehicle)
                || !_buckets.TryGet(vehicle, out var bucket)
                || !LocalVehicle.TryProjectAbove(_camera, vehicle, _config.LoadBarHeight, out var screenPoint))
            {
                _anchor.gameObject.SetActive(false);
                return;
            }

            _anchor.gameObject.SetActive(true);
            _anchor.position = screenPoint;
            _fill.fillAmount = bucket.Load / _capacity;
            _fill.color = bucket.IsFull ? _fullColor : _fillingColor;
            ShowMultiplier(_scores.IsDelivering(vehicle) ? _scores.MultiplierOf(vehicle) : 0f);
        }

        private void ShowMultiplier(float multiplier)
        {
            _multiplierBadge.SetActive(multiplier > 0f);
            if (multiplier > 0f && !Mathf.Approximately(multiplier, _shownMultiplier))
            {
                _shownMultiplier = multiplier;
                _multiplierLabel.text = HudText.Multiplier(multiplier);
            }
        }
    }
}
