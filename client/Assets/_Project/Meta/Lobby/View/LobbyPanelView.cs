using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class LobbyPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Text _playersLabel;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _leaveButton;

        public event Action StartRequested;

        public event Action LeaveRequested;

        public void SetVisible(bool visible)
        {
            _panel.SetActive(visible);
        }

        public void ShowTitle(string title, bool canStart, bool canLeave)
        {
            _titleLabel.text = title;
            _startButton.gameObject.SetActive(canStart);
            _leaveButton.gameObject.SetActive(canLeave);
        }

        public void ShowStatus(string status)
        {
            _statusLabel.text = status;
        }

        public void ShowPlayers(string players)
        {
            _playersLabel.text = players;
        }

        private void Awake()
        {
            _startButton.onClick.AddListener(OnStartClicked);
            _leaveButton.onClick.AddListener(OnLeaveClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _leaveButton.onClick.RemoveListener(OnLeaveClicked);
        }

        private void OnStartClicked()
        {
            StartRequested?.Invoke();
        }

        private void OnLeaveClicked()
        {
            LeaveRequested?.Invoke();
        }
    }
}
