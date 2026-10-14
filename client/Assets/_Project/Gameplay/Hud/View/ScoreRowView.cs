using PlowParty.Gameplay.Hud.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Gameplay.Hud.View
{
    public sealed class ScoreRowView : MonoBehaviour
    {
        [SerializeField] private Text _placeLabel;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Graphic _colorMark;
        [SerializeField] private Text _scoreLabel;
        [SerializeField] private Graphic _highlight;

        private int _shownPlace = -1;
        private string _shownNickname;
        private bool _shownLocal;
        private Color _shownColor;
        private int _shownScore = -1;

        public void Show(string nickname, Color color, bool isLocal, int score)
        {
            gameObject.SetActive(true);
            if (color != _shownColor)
            {
                _shownColor = color;
                _colorMark.color = color;
            }

            if (!ReferenceEquals(nickname, _shownNickname) || isLocal != _shownLocal)
            {
                _shownNickname = nickname;
                _shownLocal = isLocal;
                _nameLabel.text = nickname;
                _highlight.enabled = isLocal;
            }

            if (score != _shownScore)
            {
                _shownScore = score;
                _scoreLabel.text = score.ToString();
            }
        }

        public void ShowPlace(int place)
        {
            if (place != _shownPlace)
            {
                _shownPlace = place;
                _placeLabel.text = HudText.Place(place);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
