using UnityEngine;

namespace PlowParty.Gameplay.Hud.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Hud Config", fileName = nameof(HudConfig))]
    public sealed class HudConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _blizzardWarningLead = 5f;
        [SerializeField, Min(0f)] private float _goBannerDuration = 1f;
        [SerializeField, Min(0f)] private float _loadBarHeight = 2.2f;
        [SerializeField, Min(0f)] private float _scorePopupInterval = 0.3f;
        [SerializeField, Min(0.1f)] private float _scorePopupDuration = 1.2f;
        [SerializeField, Min(0f)] private float _scorePopupRise = 120f;
        [SerializeField, Min(0f)] private float _dropOffArrowMargin = 90f;

        public float BlizzardWarningLead => _blizzardWarningLead;

        public float GoBannerDuration => _goBannerDuration;

        public float LoadBarHeight => _loadBarHeight;

        public float ScorePopupInterval => _scorePopupInterval;

        public float ScorePopupDuration => _scorePopupDuration;

        public float ScorePopupRise => _scorePopupRise;

        public float DropOffArrowMargin => _dropOffArrowMargin;
    }
}
