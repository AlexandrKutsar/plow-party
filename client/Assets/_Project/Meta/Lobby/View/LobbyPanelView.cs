using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class LobbyPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Text _playersLabel;
        [SerializeField, FormerlySerializedAs("_startButton")] private Button _actionButton;
        [SerializeField] private Text _actionLabel;
        [SerializeField] private Button _leaveButton;

        public event Action ActionRequested;

        public event Action LeaveRequested;

        public void SetVisible(bool visible)
        {
            _panel.SetActive(visible);
        }

        public void ShowTitle(string title, bool canLeave)
        {
            _titleLabel.text = title;
            _leaveButton.gameObject.SetActive(canLeave);
        }

        public void ShowAction(bool visible, string label, bool interactable)
        {
            _actionButton.gameObject.SetActive(visible);
            _actionButton.interactable = interactable;
            _actionLabel.text = label;
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
            _actionButton.onClick.AddListener(OnActionClicked);
            _leaveButton.onClick.AddListener(OnLeaveClicked);
        }

        private void OnDestroy()
        {
            _actionButton.onClick.RemoveListener(OnActionClicked);
            _leaveButton.onClick.RemoveListener(OnLeaveClicked);
        }

        private void OnActionClicked()
        {
            ActionRequested?.Invoke();
        }

        private void OnLeaveClicked()
        {
            LeaveRequested?.Invoke();
        }
    }
}
