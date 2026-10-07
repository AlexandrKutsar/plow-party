using PlowParty.Gameplay.Bots.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Bot Profile Config", fileName = nameof(BotProfileConfig))]
    public sealed class BotProfileConfig : ScriptableObject
    {
        [SerializeField] private BotDifficulty _difficulty = BotDifficulty.Medium;
        [SerializeField, Min(0f)] private float _reactionDelay = 0.35f;
        [SerializeField, Min(0.05f)] private float _decisionIntervalMin = 0.3f;
        [SerializeField, Min(0.05f)] private float _decisionIntervalMax = 0.45f;
        [SerializeField, Range(0f, 90f)] private float _steeringNoiseDegrees = 12f;
        [SerializeField, Range(0f, 1f)] private float _mistakeChance = 0.12f;
        [SerializeField, Range(0f, 2f)] private float _aggression = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _greed = 0.5f;
        [SerializeField, Range(0f, 2f)] private float _caution = 0.5f;
        [SerializeField, Range(0.1f, 1f)] private float _throttleCap = 0.92f;
        [SerializeField, Range(0f, 1f)] private float _deliverEagerness = 0.15f;

        public BotDifficulty Difficulty => _difficulty;

        public BotProfile ToProfile()
        {
            return new BotProfile
            {
                Difficulty = _difficulty,
                ReactionDelay = _reactionDelay,
                DecisionIntervalMin = _decisionIntervalMin,
                DecisionIntervalMax = Mathf.Max(_decisionIntervalMin, _decisionIntervalMax),
                SteeringNoiseDegrees = _steeringNoiseDegrees,
                MistakeChance = _mistakeChance,
                Aggression = _aggression,
                Greed = _greed,
                Caution = _caution,
                ThrottleCap = _throttleCap,
                DeliverEagerness = _deliverEagerness,
            };
        }
    }
}
