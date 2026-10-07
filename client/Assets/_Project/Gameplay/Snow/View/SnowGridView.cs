using PlowParty.Gameplay.Snow.Config;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Snow.View
{
    public sealed class SnowGridView : MonoBehaviour
    {
        private const float BladeSampleInterval = 1f / 60f;

        private static readonly int HeightMapId = Shader.PropertyToID("_HeightMap");
        private static readonly int HeightScaleId = Shader.PropertyToID("_HeightScale");
        private static readonly int SnowLineId = Shader.PropertyToID("_SnowLine");
        private static readonly int CellSizeId = Shader.PropertyToID("_CellSize");

        [SerializeField] private SnowGridDriver _driver;
        [SerializeField] private Renderer _surface;
        [SerializeField] private ParticleSystem _pileBurst;

        private VehicleRegistry _registry;
        private IScrapeLimit _limit;
        private SnowConfig _config;
        private SnowSettings _settings;
        private SnowGrid _shown;
        private SnowHeightField _heights;
        private SnowPileBursts _bursts;
        private Mesh _mesh;
        private int[] _paintedWords;
        private SnowBlade[] _recentBlades;
        private float[] _recentBladeTimes;
        private int _nextRecentBlade;
        private float _lastBladeSampleTime = float.NegativeInfinity;
        private NetworkVehicle _localVehicle;

        [Inject]
        public void Construct(VehicleRegistry registry, SnowConfig config, IScrapeLimit limit)
        {
            _registry = registry;
            _config = config;
            _limit = limit;
        }

        private void LateUpdate()
        {
            if (_driver == null || !_driver.IsReady)
            {
                return;
            }

            if (_shown == null)
            {
                CreateSurface();
            }

            _driver.CopyWordsTo(_shown);
            PreClearUnderLocalBlade();
            UpdateChangedWords();
            _heights.Advance(Time.deltaTime);
            _bursts.Show(_registry, _driver);
        }

        private void OnDestroy()
        {
            if (_heights != null)
            {
                Destroy(_heights.Texture);
            }

            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        private void CreateSurface()
        {
            _settings = _config.ToSettings();
            _shown = new SnowGrid(_settings, VehicleArena.Create(), 0);
            _driver.CopyWordsTo(_shown);
            _heights = new SnowHeightField(_shown, _settings, _config);
            _bursts = new SnowPileBursts(_pileBurst, transform, _settings.BladeForwardOffset);
            _paintedWords = new int[_shown.WordCount];
            for (var i = 0; i < _paintedWords.Length; i++)
            {
                _paintedWords[i] = _shown.GetWord(i);
            }

            var bladeCapacity = Mathf.CeilToInt(_config.PreClearDuration / BladeSampleInterval) + 1;
            _recentBlades = new SnowBlade[bladeCapacity];
            _recentBladeTimes = new float[bladeCapacity];
            for (var i = 0; i < bladeCapacity; i++)
            {
                _recentBladeTimes[i] = float.NegativeInfinity;
            }

            _mesh = SnowSurfaceMesh.Build(_shown.Width, _shown.Height, _config.VerticesPerCell, _heights.MaxHeight);
            _surface.GetComponent<MeshFilter>().sharedMesh = _mesh;
            FitSurfaceToGrid();
            var block = new MaterialPropertyBlock();
            _surface.GetPropertyBlock(block);
            block.SetTexture(HeightMapId, _heights.Texture);
            block.SetFloat(HeightScaleId, _heights.MaxHeight);
            block.SetFloat(SnowLineId, _heights.FullSnowLine);
            block.SetFloat(CellSizeId, _settings.CellSize);
            _surface.SetPropertyBlock(block);
        }

        private void FitSurfaceToGrid()
        {
            var surface = _surface.transform;
            var centre = _settings.Origin + _settings.Size * 0.5f;
            surface.SetPositionAndRotation(new Vector3(centre.x, surface.position.y, centre.y), Quaternion.identity);
            surface.localScale = new Vector3(_settings.Size.x, 1f, _settings.Size.y);
        }

        private void PreClearUnderLocalBlade()
        {
            if (Time.time - _lastBladeSampleTime >= BladeSampleInterval)
            {
                SampleLocalBlade();
            }

            var oldest = Time.time - _config.PreClearDuration;
            var limit = _localVehicle != null ? _limit.LimitFor(_localVehicle) : 0;
            for (var i = 0; i < _recentBlades.Length && limit > 0; i++)
            {
                if (_recentBladeTimes[i] >= oldest)
                {
                    limit -= _shown.Scrape(_recentBlades[i], limit);
                }
            }
        }

        private void SampleLocalBlade()
        {
            var vehicles = _registry.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].HasInputAuthority)
                {
                    _localVehicle = vehicles[i];
                    _recentBlades[_nextRecentBlade] = SnowBlade.Ahead(vehicles[i].Position, vehicles[i].Forward, _settings);
                    _recentBladeTimes[_nextRecentBlade] = Time.time;
                    _nextRecentBlade = (_nextRecentBlade + 1) % _recentBlades.Length;
                    _lastBladeSampleTime = Time.time;
                }
            }
        }

        private void UpdateChangedWords()
        {
            for (var word = 0; word < _paintedWords.Length; word++)
            {
                var value = _shown.GetWord(word);
                if (value == _paintedWords[word])
                {
                    continue;
                }

                _paintedWords[word] = value;
                var first = word * SnowGrid.CellsPerWord;
                var last = Mathf.Min(first + SnowGrid.CellsPerWord, _shown.Width * _shown.Height);
                for (var cell = first; cell < last; cell++)
                {
                    _heights.SetDepth(cell, _shown.GetDepth(cell));
                }
            }
        }
    }
}
