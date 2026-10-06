using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Snow.Config;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class BlizzardAnnouncementView : MonoBehaviour
    {
        [SerializeField] private GameObject _banner;
        [SerializeField] private Text _label;

        private IMatchClock _match;
        private BlizzardWarning _warning;
        private int _shownSeconds = -1;

        [Inject]
        public void Construct(IMatchClock match, SnowConfig snowConfig, HudConfig config)
        {
            _match = match;
            _warning = new BlizzardWarning(snowConfig.ToSettings().BlizzardTimes, config.BlizzardWarningLead);
        }

        private void Update()
        {
            var secondsLeft = 0f;
            var visible = _match.IsRunning
                && _match.Phase == MatchPhase.Playing
                && _warning.TryGetSecondsLeft(_match.PlayingElapsed, out secondsLeft);
            _banner.SetActive(visible);
            if (visible)
            {
                Show(HudText.WholeSecondsLeft(secondsLeft));
            }
        }

        private void Show(int seconds)
        {
            if (seconds != _shownSeconds)
            {
                _shownSeconds = seconds;
                _label.text = $"Blizzard in {seconds}!";
            }
        }
    }
}
