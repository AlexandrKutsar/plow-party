using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class SearchView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Text _stopwatchLabel;
        [SerializeField] private Button _stopButton;
        [SerializeField] private Text _stopLabel;

        public event Action StopRequested;

        public void SetVisible(bool visible)
        {
            _panel.SetActive(visible);
        }

        public void Show(string title, string stopLabel, bool canStop)
        {
            _titleLabel.text = title;
            _stopLabel.text = stopLabel;
            _stopButton.gameObject.SetActive(canStop);
        }

        public void ShowStopwatch(string text)
        {
            _stopwatchLabel.text = text;
        }

        private void Awake()
        {
            _stopButton.onClick.AddListener(OnStopClicked);
        }

        private void OnDestroy()
        {
            _stopButton.onClick.RemoveListener(OnStopClicked);
        }

        private void OnStopClicked()
        {
            StopRequested?.Invoke();
        }
    }
}
