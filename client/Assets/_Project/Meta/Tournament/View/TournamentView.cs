using System;
using PlowParty.Meta.Tournament.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Tournament.View
{
    public sealed class TournamentView : MonoBehaviour
    {
        [SerializeField] private Text _summaryLabel;
        [SerializeField] private Text _medalLabel;
        [SerializeField] private StandingRowView[] _rows;
        [SerializeField] private Button _refreshButton;

        public event Action RefreshRequested;

        public void ShowLoading()
        {
            _summaryLabel.text = TournamentText.Loading;
            _refreshButton.interactable = false;
        }

        public void ShowBoard(TournamentBoard board)
        {
            _refreshButton.interactable = true;
            _summaryLabel.text = TournamentText.Summary(board);
            _medalLabel.text = board == null ? string.Empty : TournamentText.MedalLabel(board.Medal);
            for (var i = 0; i < _rows.Length; i++)
            {
                if (board != null && i < board.Rows.Count)
                {
                    _rows[i].Show(board.Rows[i]);
                }
                else
                {
                    _rows[i].Hide();
                }
            }
        }

        private void Awake()
        {
            _refreshButton.onClick.AddListener(OnRefreshClicked);
        }

        private void OnDestroy()
        {
            _refreshButton.onClick.RemoveListener(OnRefreshClicked);
        }

        private void OnRefreshClicked()
        {
            RefreshRequested?.Invoke();
        }
    }
}
