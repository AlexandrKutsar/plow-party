using System;
using PlowParty.Meta.Lobby.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class MenuTabsView : MonoBehaviour
    {
        [SerializeField] private Button _playTab;
        [SerializeField] private Button _tournamentTab;
        [SerializeField] private GameObject[] _playPage;
        [SerializeField] private GameObject[] _tournamentPage;

        public event Action<MenuTab> TabRequested;

        public void Show(MenuTab tab)
        {
            _playTab.interactable = tab != MenuTab.Play;
            _tournamentTab.interactable = tab != MenuTab.Tournament;
            SetActive(_playPage, tab == MenuTab.Play);
            SetActive(_tournamentPage, tab == MenuTab.Tournament);
        }

        private static void SetActive(GameObject[] page, bool active)
        {
            foreach (var part in page)
            {
                part.SetActive(active);
            }
        }

        private void Awake()
        {
            _playTab.onClick.AddListener(OnPlayClicked);
            _tournamentTab.onClick.AddListener(OnTournamentClicked);
        }

        private void OnDestroy()
        {
            _playTab.onClick.RemoveListener(OnPlayClicked);
            _tournamentTab.onClick.RemoveListener(OnTournamentClicked);
        }

        private void OnPlayClicked()
        {
            TabRequested?.Invoke(MenuTab.Play);
        }

        private void OnTournamentClicked()
        {
            TabRequested?.Invoke(MenuTab.Tournament);
        }
    }
}
