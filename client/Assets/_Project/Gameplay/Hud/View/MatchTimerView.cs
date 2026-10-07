using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Hud.Simulation;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class MatchTimerView : MonoBehaviour
    {
        [SerializeField] private GameObject _clockPanel;
        [SerializeField] private Text _clockLabel;
        [SerializeField] private Text _countdownLabel;

        private IMatchClock _match;
        private HudConfig _config;
        private int _shownClock = -1;
        private int _shownCountdown = -1;

        [Inject]
        public void Construct(IMatchClock match, HudConfig config)
        {
            _match = match;
            _config = config;
        }

        private void Update()
        {
            var phase = _match.IsRunning ? _match.Phase : MatchPhase.Results;
            var showsClock = phase == MatchPhase.Countdown || phase == MatchPhase.Playing;
            _clockPanel.SetActive(showsClock);
            if (showsClock)
            {
                ShowClock(HudText.WholeSecondsLeft(_match.PlayingRemaining));
            }

            ShowCountdown(phase);
        }

        private void ShowClock(int seconds)
        {
            if (seconds != _shownClock)
            {
                _shownClock = seconds;
                _clockLabel.text = HudText.Clock(seconds);
            }
        }

        private void ShowCountdown(MatchPhase phase)
        {
            var value = CountdownValue(phase);
            var label = _countdownLabel.gameObject;
            label.SetActive(value >= 0);
            if (value < 0 || value == _shownCountdown)
            {
                return;
            }

            _shownCountdown = value;
            _countdownLabel.text = value == 0 ? HudText.Go : value.ToString();
        }

        private int CountdownValue(MatchPhase phase)
        {
            if (phase == MatchPhase.Countdown)
            {
                return Mathf.Max(1, HudText.WholeSecondsLeft(_match.PhaseRemaining));
            }

            if (phase == MatchPhase.Playing && _match.PlayingElapsed < _config.GoBannerDuration)
            {
                return 0;
            }

            return -1;
        }
    }
}
