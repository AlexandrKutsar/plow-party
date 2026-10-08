using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class PlayMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _quickPlayButton;
        [SerializeField, FormerlySerializedAs("_createRoomButton")] private Button _createPartyButton;
        [SerializeField] private InputField _codeInput;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Text _failureLabel;

        public event Action QuickPlayRequested;

        public event Action CreatePartyRequested;

        public event Action<string> JoinRequested;

        public void Show(bool visible, string failure)
        {
            _panel.SetActive(visible);
            _failureLabel.text = failure ?? string.Empty;
        }

        private void Awake()
        {
            _quickPlayButton.onClick.AddListener(OnQuickPlayClicked);
            _createPartyButton.onClick.AddListener(OnCreatePartyClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnDestroy()
        {
            _quickPlayButton.onClick.RemoveListener(OnQuickPlayClicked);
            _createPartyButton.onClick.RemoveListener(OnCreatePartyClicked);
            _joinButton.onClick.RemoveListener(OnJoinClicked);
        }

        private void OnQuickPlayClicked()
        {
            QuickPlayRequested?.Invoke();
        }

        private void OnCreatePartyClicked()
        {
            CreatePartyRequested?.Invoke();
        }

        private void OnJoinClicked()
        {
            JoinRequested?.Invoke(_codeInput.text);
        }
    }
}
