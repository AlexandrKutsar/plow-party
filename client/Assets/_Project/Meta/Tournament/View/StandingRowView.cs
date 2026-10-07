using PlowParty.Meta.Tournament.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PlowParty.Meta.Tournament.View
{
    public sealed class StandingRowView : MonoBehaviour
    {
        [SerializeField] private Text _rankLabel;
        [SerializeField] private Text _nicknameLabel;
        [SerializeField] private Text _scoreLabel;
        [SerializeField] private Graphic _highlight;

        public void Show(Standing standing)
        {
            gameObject.SetActive(true);
            _rankLabel.text = TournamentText.Rank(standing);
            _nicknameLabel.text = standing.Nickname;
            _scoreLabel.text = standing.IsGap ? string.Empty : standing.Score.ToString();
            _highlight.enabled = standing.IsMe;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
