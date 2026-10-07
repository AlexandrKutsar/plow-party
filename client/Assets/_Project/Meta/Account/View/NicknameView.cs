using System;
using PlowParty.Meta.Account.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Account.View
{
    public sealed class NicknameView : MonoBehaviour
    {
        private const string OnlineText = "в сети";
        private const string OfflineText = "офлайн";

        [SerializeField] private Text _nicknameLabel;
        [SerializeField] private Text _connectionLabel;
        [SerializeField] private Button _editButton;
        [SerializeField] private GameObject _editPanel;
        [SerializeField] private InputField _input;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Text _errorLabel;

        public event Action EditRequested;

        public event Action<string> SaveRequested;

        public event Action CancelRequested;

        public void ShowAccount(string nickname, bool online)
        {
            _nicknameLabel.text = nickname;
            _connectionLabel.text = online ? OnlineText : OfflineText;
        }

        public void OpenEditor(string nickname)
        {
            _editPanel.SetActive(true);
            _input.text = nickname;
            ShowError(string.Empty);
            SetBusy(false);
        }

        public void CloseEditor()
        {
            _editPanel.SetActive(false);
        }

        public void ShowError(string error)
        {
            _errorLabel.text = error;
        }

        public void SetBusy(bool busy)
        {
            _saveButton.interactable = !busy;
            _input.interactable = !busy;
        }

        private void Awake()
        {
            _input.characterLimit = NicknameRules.MaxLength;
            _editPanel.SetActive(false);
            _editButton.onClick.AddListener(OnEditClicked);
            _saveButton.onClick.AddListener(OnSaveClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDestroy()
        {
            _editButton.onClick.RemoveListener(OnEditClicked);
            _saveButton.onClick.RemoveListener(OnSaveClicked);
            _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        private void OnEditClicked()
        {
            EditRequested?.Invoke();
        }

        private void OnSaveClicked()
        {
            SaveRequested?.Invoke(_input.text);
        }

        private void OnCancelClicked()
        {
            CancelRequested?.Invoke();
        }
    }
}
