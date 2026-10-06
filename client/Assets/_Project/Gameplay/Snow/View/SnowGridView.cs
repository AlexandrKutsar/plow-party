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
        private const int UnlimitedRoom = int.MaxValue;

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

        [SerializeField] private SnowGridDriver _driver;
        [SerializeField] private Renderer _surface;
        [SerializeField] private Color32 _groundColor = new Color32(92, 96, 104, 255);
        [SerializeField] private Color32 _snowColor = new Color32(240, 246, 255, 255);
        [SerializeField] private Color32 _pileColor = new Color32(196, 222, 255, 255);

        private VehicleRegistry _registry;
        private SnowConfig _config;
        private SnowSettings _settings;
        private SnowGrid _shown;
        private Texture2D _texture;
        private Color32[] _pixels;
        private int[] _paintedWords;
        private MaterialPropertyBlock _block;
        private SnowBlade[] _recentBlades;
        private float[] _recentBladeTimes;
        private int _nextRecentBlade;
        private float _lastBladeSampleTime = float.NegativeInfinity;

        [Inject]
        public void Construct(VehicleRegistry registry, SnowConfig config)
        {
            _registry = registry;
            _config = config;
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
            Paint();
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
            }
        }

        private void CreateSurface()
        {
            _settings = _config.ToSettings();
            _shown = new SnowGrid(_settings, VehicleArena.Create(), 0);
            _texture = new Texture2D(_shown.Width, _shown.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            _pixels = new Color32[_shown.Width * _shown.Height];
            _paintedWords = new int[_shown.WordCount];
            for (var i = 0; i < _paintedWords.Length; i++)
            {
                _paintedWords[i] = ~_shown.GetWord(i);
            }

            var bladeCapacity = Mathf.CeilToInt(_config.PreClearDuration / BladeSampleInterval) + 1;
            _recentBlades = new SnowBlade[bladeCapacity];
            _recentBladeTimes = new float[bladeCapacity];
            for (var i = 0; i < bladeCapacity; i++)
            {
                _recentBladeTimes[i] = float.NegativeInfinity;
            }

            FitSurfaceToGrid();
            _block = new MaterialPropertyBlock();
            _surface.GetPropertyBlock(_block);
            _block.SetTexture(BaseMapId, _texture);
            _surface.SetPropertyBlock(_block);
        }

        private void FitSurfaceToGrid()
        {
            var surface = _surface.transform;
            var centre = _settings.Origin + _settings.Size * 0.5f;
            surface.position = new Vector3(centre.x, surface.position.y, centre.y);
            surface.localScale = new Vector3(_settings.Size.x, _settings.Size.y, 1f);
        }

        private void PreClearUnderLocalBlade()
        {
            if (Time.time - _lastBladeSampleTime >= BladeSampleInterval)
            {
                SampleLocalBlade();
            }

            var oldest = Time.time - _config.PreClearDuration;
            for (var i = 0; i < _recentBlades.Length; i++)
            {
                if (_recentBladeTimes[i] >= oldest)
                {
                    _shown.Scrape(_recentBlades[i], UnlimitedRoom);
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
                    _recentBlades[_nextRecentBlade] = SnowBlade.Ahead(vehicles[i].Position, vehicles[i].Forward, _settings);
                    _recentBladeTimes[_nextRecentBlade] = Time.time;
                    _nextRecentBlade = (_nextRecentBlade + 1) % _recentBlades.Length;
                    _lastBladeSampleTime = Time.time;
                }
            }
        }

        private void Paint()
        {
            var changed = false;
            for (var word = 0; word < _paintedWords.Length; word++)
            {
                var value = _shown.GetWord(word);
                if (value == _paintedWords[word])
                {
                    continue;
                }

                _paintedWords[word] = value;
                PaintWord(word);
                changed = true;
            }

            if (changed)
            {
                _texture.SetPixels32(_pixels);
                _texture.Apply(false);
            }
        }

        private void PaintWord(int word)
        {
            var first = word * SnowGrid.CellsPerWord;
            var last = Mathf.Min(first + SnowGrid.CellsPerWord, _pixels.Length);
            for (var cell = first; cell < last; cell++)
            {
                _pixels[cell] = ColourOf(_shown.GetDepth(cell));
            }
        }

        private Color32 ColourOf(int depth)
        {
            var full = _settings.FullDepth;
            if (depth <= full)
            {
                return Color32.Lerp(_groundColor, _snowColor, (float)depth / full);
            }

            return Color32.Lerp(_snowColor, _pileColor, (float)(depth - full) / (SnowGrid.MaxDepth - full));
        }
    }
}
