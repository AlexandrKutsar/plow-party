using System;
using PlowParty.Meta.Lobby.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class CustomGameSetupView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _notCountedLabel;
        [SerializeField] private SetupSlotRowView[] _slotRows;
        [SerializeField] private GameObject _lookRoot;
        [SerializeField] private Button[] _colorButtons;
        [SerializeField] private Button _previousCritterButton;
        [SerializeField] private Button _nextCritterButton;
        [SerializeField] private Text _critterLabel;
        [SerializeField] private float _takenAlpha = 0.25f;
        [SerializeField] private float _mineScale = 1.3f;

        private Action[] _colorClicks;

        public event Action<int> AddBotRequested;

        public event Action<int> DifficultyRequested;

        public event Action<int> RemoveBotRequested;

        public event Action<int> ColorRequested;

        public event Action<int> CritterStepRequested;

        public void Show(PartyPanelState state, Func<int, Color> colorOf)
        {
            _panel.SetActive(state.ShowSetup);
            if (!state.ShowSetup)
            {
                return;
            }

            _notCountedLabel.text = state.NotCountedText;
            for (var i = 0; i < _slotRows.Length; i++)
            {
                var visible = i < state.Slots.Count;
                _slotRows[i].gameObject.SetActive(visible);
                if (visible)
                {
                    var row = state.Slots[i];
                    _slotRows[i].Show(row, row.HasColor ? colorOf(row.Color) : Color.clear);
                }
            }

            _lookRoot.SetActive(state.CanPickLook);
            for (var i = 0; i < _colorButtons.Length; i++)
            {
                var visible = i < state.Colors.Count;
                _colorButtons[i].gameObject.SetActive(visible);
                if (visible)
                {
                    ShowColor(_colorButtons[i], state.Colors[i], colorOf(i));
                }
            }

            _critterLabel.text = state.CritterName;
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        private void ShowColor(Button button, ColorChoice choice, Color color)
        {
            color.a = choice.IsTaken ? _takenAlpha : 1f;
            button.image.color = color;
            button.interactable = !choice.IsTaken && !choice.IsMine;
            button.transform.localScale = Vector3.one * (choice.IsMine ? _mineScale : 1f);
        }

        private void Awake()
        {
            foreach (var row in _slotRows)
            {
                row.AddBotRequested += OnAddBot;
                row.DifficultyRequested += OnDifficulty;
                row.RemoveBotRequested += OnRemoveBot;
            }

            _colorClicks = new Action[_colorButtons.Length];
            for (var i = 0; i < _colorButtons.Length; i++)
            {
                var color = i;
                _colorClicks[i] = () => ColorRequested?.Invoke(color);
                _colorButtons[i].onClick.AddListener(_colorClicks[i].Invoke);
            }

            _previousCritterButton.onClick.AddListener(OnPreviousCritter);
            _nextCritterButton.onClick.AddListener(OnNextCritter);
        }

        private void OnDestroy()
        {
            foreach (var row in _slotRows)
            {
                row.AddBotRequested -= OnAddBot;
                row.DifficultyRequested -= OnDifficulty;
                row.RemoveBotRequested -= OnRemoveBot;
            }

            foreach (var button in _colorButtons)
            {
                button.onClick.RemoveAllListeners();
            }

            _previousCritterButton.onClick.RemoveListener(OnPreviousCritter);
            _nextCritterButton.onClick.RemoveListener(OnNextCritter);
        }

        private void OnAddBot(int slot)
        {
            AddBotRequested?.Invoke(slot);
        }

        private void OnDifficulty(int slot)
        {
            DifficultyRequested?.Invoke(slot);
        }

        private void OnRemoveBot(int slot)
        {
            RemoveBotRequested?.Invoke(slot);
        }

        private void OnPreviousCritter()
        {
            CritterStepRequested?.Invoke(-1);
        }

        private void OnNextCritter()
        {
            CritterStepRequested?.Invoke(1);
        }
    }
}
