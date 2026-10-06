using PlowParty.Gameplay.Bucket.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bucket.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Bucket Config", fileName = nameof(BucketConfig))]
    public sealed class BucketConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _capacity = 100;
        [SerializeField, Min(1)] private int _stepsPerLoad = 6;
        [SerializeField, Range(0f, 1f)] private float _maxSpeedPenalty = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _spillShare = 0.3f;

        public BucketSettings ToSettings()
        {
            return new BucketSettings
            {
                Capacity = _capacity,
                StepsPerLoad = _stepsPerLoad,
                MaxSpeedPenalty = _maxSpeedPenalty,
                SpillShare = _spillShare,
            };
        }
    }
}
