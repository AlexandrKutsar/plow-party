using System;
using PlowParty.Meta.Lobby.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class SetupSlotRowView : MonoBehaviour
    {
        [SerializeField] private Image _colorDot;
        [SerializeField] private Text _label;
        [SerializeField] private Button _addBotButton;
        [SerializeField] private Button _difficultyButton;
        [SerializeField] private Button _removeBotButton;

        private int _slot;

        public event Action<int> AddBotRequested;

        public event Action<int> DifficultyRequested;

        public event Action<int> RemoveBotRequested;

        public void Show(SetupSlotRow row, Color color)
        {
            _slot = row.Slot;
            _colorDot.enabled = row.HasColor;
            _colorDot.color = color;
            _label.text = row.Label;
            _addBotButton.gameObject.SetActive(row.CanAddBot);
            _difficultyButton.gameObject.SetActive(row.CanChangeBot);
            _removeBotButton.gameObject.SetActive(row.CanChangeBot);
        }

        private void Awake()
        {
            _addBotButton.onClick.AddListener(OnAddBotClicked);
            _difficultyButton.onClick.AddListener(OnDifficultyClicked);
            _removeBotButton.onClick.AddListener(OnRemoveBotClicked);
        }

        private void OnDestroy()
        {
            _addBotButton.onClick.RemoveListener(OnAddBotClicked);
            _difficultyButton.onClick.RemoveListener(OnDifficultyClicked);
            _removeBotButton.onClick.RemoveListener(OnRemoveBotClicked);
        }

        private void OnAddBotClicked()
        {
            AddBotRequested?.Invoke(_slot);
        }

        private void OnDifficultyClicked()
        {
            DifficultyRequested?.Invoke(_slot);
        }

        private void OnRemoveBotClicked()
        {
            RemoveBotRequested?.Invoke(_slot);
        }
    }
}
