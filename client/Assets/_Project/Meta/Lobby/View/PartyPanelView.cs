using System;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class PartyPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Button _copyButton;
        [SerializeField] private PartyMemberRowView[] _rows;
        [SerializeField] private Button _quickPlayModeButton;
        [SerializeField] private Button _customGameModeButton;
        [SerializeField] private Color _chosenModeColor = Color.white;
        [SerializeField] private Color _otherModeColor = Color.gray;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Text _readyLabel;
        [SerializeField] private Button _searchButton;
        [SerializeField] private Button _leaveButton;

        private string _code;

        public event Action<int> RemoveRequested;

        public event Action<PartyMode> ModeRequested;

        public event Action ReadyRequested;

        public event Action SearchRequested;

        public event Action LeaveRequested;

        public int RowCount => _rows.Length;

        public void SetVisible(bool visible)
        {
            _panel.SetActive(visible);
        }

        public void Show(PartyPanelState state)
        {
            _code = state.Code;
            _titleLabel.text = state.Title;
            _copyButton.gameObject.SetActive(state.CanCopy);
            ShowMode(_quickPlayModeButton, state.Mode == PartyMode.QuickPlay, state.CanPickQuickPlay);
            ShowMode(_customGameModeButton, state.Mode == PartyMode.CustomGame, state.CanPickCustomGame);
            _readyButton.gameObject.SetActive(state.ShowReady);
            _readyLabel.text = state.ReadyLabel;
            _searchButton.gameObject.SetActive(state.ShowSearch);
            _searchButton.interactable = state.CanSearch;
        }

        public void ShowRow(int index, PartyRow row, Color color)
        {
            _rows[index].Show(row, color);
        }

        public void HideRow(int index)
        {
            _rows[index].Hide();
        }

        private void ShowMode(Button button, bool chosen, bool canPick)
        {
            button.interactable = canPick && !chosen;
            button.image.color = chosen ? _chosenModeColor : _otherModeColor;
        }

        private void Awake()
        {
            _copyButton.onClick.AddListener(OnCopyClicked);
            _quickPlayModeButton.onClick.AddListener(OnQuickPlayModeClicked);
            _customGameModeButton.onClick.AddListener(OnCustomGameModeClicked);
            _readyButton.onClick.AddListener(OnReadyClicked);
            _searchButton.onClick.AddListener(OnSearchClicked);
            _leaveButton.onClick.AddListener(OnLeaveClicked);
            foreach (var row in _rows)
            {
                row.RemoveRequested += OnRemoveRequested;
            }
        }

        private void OnDestroy()
        {
            _copyButton.onClick.RemoveListener(OnCopyClicked);
            _quickPlayModeButton.onClick.RemoveListener(OnQuickPlayModeClicked);
            _customGameModeButton.onClick.RemoveListener(OnCustomGameModeClicked);
            _readyButton.onClick.RemoveListener(OnReadyClicked);
            _searchButton.onClick.RemoveListener(OnSearchClicked);
            _leaveButton.onClick.RemoveListener(OnLeaveClicked);
            foreach (var row in _rows)
            {
                row.RemoveRequested -= OnRemoveRequested;
            }
        }

        private void OnCopyClicked()
        {
            GUIUtility.systemCopyBuffer = _code;
        }

        private void OnRemoveRequested(int memberId)
        {
            RemoveRequested?.Invoke(memberId);
        }

        private void OnQuickPlayModeClicked()
        {
            ModeRequested?.Invoke(PartyMode.QuickPlay);
        }

        private void OnCustomGameModeClicked()
        {
            ModeRequested?.Invoke(PartyMode.CustomGame);
        }

        private void OnReadyClicked()
        {
            ReadyRequested?.Invoke();
        }

        private void OnSearchClicked()
        {
            SearchRequested?.Invoke();
        }

        private void OnLeaveClicked()
        {
            LeaveRequested?.Invoke();
        }
    }
}
