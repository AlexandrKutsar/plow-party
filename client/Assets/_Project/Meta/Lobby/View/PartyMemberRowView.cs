using System;
using PlowParty.Meta.Lobby.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class PartyMemberRowView : MonoBehaviour
    {
        [SerializeField] private Image _colorDot;
        [SerializeField] private Text _nicknameLabel;
        [SerializeField] private GameObject _crown;
        [SerializeField] private Text _readyLabel;
        [SerializeField] private Button _removeButton;

        private int _memberId;

        public event Action<int> RemoveRequested;

        public void Show(PartyRow row, Color color)
        {
            _memberId = row.MemberId;
            gameObject.SetActive(true);
            _colorDot.color = color;
            _nicknameLabel.text = row.Nickname;
            _crown.SetActive(row.HasCrown);
            _readyLabel.text = row.ReadyText;
            _removeButton.gameObject.SetActive(row.CanRemove);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            _removeButton.onClick.AddListener(OnRemoveClicked);
        }

        private void OnDestroy()
        {
            _removeButton.onClick.RemoveListener(OnRemoveClicked);
        }

        private void OnRemoveClicked()
        {
            RemoveRequested?.Invoke(_memberId);
        }
    }
}
