using PlowParty.Gameplay.Snow.Config;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Snow.View
{
    public sealed class SnowSurfaceView : MonoBehaviour
    {
        private const int RecentBladeCapacity = 64;

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

        [SerializeField] private SnowGridDriver _driver;
        [SerializeField] private Renderer _surface;
        [SerializeField] private Color32 _groundColor = new Color32(92, 96, 104, 255);
        [SerializeField] private Color32 _snowColor = new Color32(240, 246, 255, 255);
        [SerializeField] private Color32 _pileColor = new Color32(196, 222, 255, 255);

        private readonly SnowBlade[] _recentBlades = new SnowBlade[RecentBladeCapacity];
        private readonly float[] _recentBladeTimes = new float[RecentBladeCapacity];

        private VehicleRegistry _registry;
        private SnowConfig _config;
        private SnowGrid _shown;
        private Texture2D _texture;
        private Color32[] _pixels;
        private int[] _paintedWords;
        private MaterialPropertyBlock _block;
        private int _nextRecentBlade;

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
            _shown = new SnowGrid(_driver.Settings, VehicleArena.Create(), 0);
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

            FitSurfaceToGrid(_driver.Settings);
            _block = new MaterialPropertyBlock();
            _surface.GetPropertyBlock(_block);
            _block.SetTexture(BaseMapId, _texture);
            _surface.SetPropertyBlock(_block);
        }

        private void FitSurfaceToGrid(SnowSettings settings)
        {
            var surface = _surface.transform;
            var centre = settings.Origin + settings.Size * 0.5f;
            surface.position = new Vector3(centre.x, surface.position.y, centre.y);
            surface.localScale = new Vector3(settings.Size.x, settings.Size.y, 1f);
        }

        private void PreClearUnderLocalBlade()
        {
            var vehicles = _registry.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].HasInputAuthority)
                {
                    RememberBlade(SnowBlade.Ahead(vehicles[i].Position, vehicles[i].Forward, _driver.Settings));
                }
            }

            var oldest = Time.time - _config.PreClearDuration;
            for (var i = 0; i < RecentBladeCapacity; i++)
            {
                if (_recentBladeTimes[i] > 0f && _recentBladeTimes[i] >= oldest)
                {
                    _shown.Scrape(_recentBlades[i], int.MaxValue);
                }
            }
        }

        private void RememberBlade(SnowBlade blade)
        {
            _recentBlades[_nextRecentBlade] = blade;
            _recentBladeTimes[_nextRecentBlade] = Time.time;
            _nextRecentBlade = (_nextRecentBlade + 1) % RecentBladeCapacity;
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
            for (var index = first; index < last; index++)
            {
                _pixels[index] = ColourOf(_shown.GetDepth(index % _shown.Width, index / _shown.Width));
            }
        }

        private Color32 ColourOf(int depth)
        {
            var full = _driver.Settings.FullDepth;
            if (depth <= full)
            {
                return Color32.Lerp(_groundColor, _snowColor, (float)depth / full);
            }

            return Color32.Lerp(_snowColor, _pileColor, (float)(depth - full) / (SnowGrid.MaxDepth - full));
        }
    }
}
