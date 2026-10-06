using PlowParty.Gameplay.Hud.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class ScoreRowView : MonoBehaviour
    {
        [SerializeField] private Text _placeLabel;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Text _scoreLabel;
        [SerializeField] private Graphic _highlight;

        private int _shownPlace = -1;
        private int _shownSlot = -1;
        private bool _shownLocal;
        private int _shownScore = -1;

        public void Show(int slot, bool isLocal, int score)
        {
            Show(0, slot, isLocal, score);
        }

        public void Show(int place, int slot, bool isLocal, int score)
        {
            gameObject.SetActive(true);
            if (_placeLabel != null && place != _shownPlace)
            {
                _shownPlace = place;
                _placeLabel.text = HudText.Place(place);
            }

            if (slot != _shownSlot || isLocal != _shownLocal)
            {
                _shownSlot = slot;
                _shownLocal = isLocal;
                _nameLabel.text = HudText.ParticipantName(slot, isLocal);
                _highlight.enabled = isLocal;
            }

            if (score != _shownScore)
            {
                _shownScore = score;
                _scoreLabel.text = score.ToString();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
