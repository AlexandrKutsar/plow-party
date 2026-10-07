using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class PlayMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _quickPlayButton;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private InputField _codeInput;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Text _failureLabel;

        public event Action QuickPlayRequested;

        public event Action CreateRoomRequested;

        public event Action<string> JoinRequested;

        public void Show(bool visible, string failure)
        {
            _panel.SetActive(visible);
            _failureLabel.text = failure ?? string.Empty;
        }

        private void Awake()
        {
            _quickPlayButton.onClick.AddListener(OnQuickPlayClicked);
            _createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnDestroy()
        {
            _quickPlayButton.onClick.RemoveListener(OnQuickPlayClicked);
            _createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
            _joinButton.onClick.RemoveListener(OnJoinClicked);
        }

        private void OnQuickPlayClicked()
        {
            QuickPlayRequested?.Invoke();
        }

        private void OnCreateRoomClicked()
        {
            CreateRoomRequested?.Invoke();
        }

        private void OnJoinClicked()
        {
            JoinRequested?.Invoke(_codeInput.text);
        }
    }
}
