using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotUtility
    {
        private const int ActionCount = (int)BotAction.Evade + 1;

        private readonly BotSettings _settings;

        public BotUtility(BotSettings settings)
        {
            _settings = settings;
        }

        public BotAction Choose(BotSituation situation, BotProfile profile, BotAction current, double mistakeRoll)
        {
            var best = BestAction(situation, profile, current, null);
            if (mistakeRoll >= profile.MistakeChance)
            {
                return best;
            }

            var second = BestAction(situation, profile, current, best);
            return Score(second, situation, profile) > 0f ? second : best;
        }

        public float Score(BotAction action, BotSituation situation, BotProfile profile)
        {
            switch (action)
            {
                case BotAction.Collect:
                    return CollectScore(situation);
                case BotAction.Deliver:
                    return DeliverScore(situation, profile);
                case BotAction.ChasePile:
                    return PileScore(situation);
                case BotAction.Ram:
                    return RamScore(situation, profile);
                default:
                    return EvadeScore(situation, profile);
            }
        }

        private BotAction BestAction(BotSituation situation, BotProfile profile, BotAction current, BotAction? excluded)
        {
            var best = BotAction.Collect;
            var bestScore = float.MinValue;
            for (var i = 0; i < ActionCount; i++)
            {
                var action = (BotAction)i;
                if (action == excluded)
                {
                    continue;
                }

                var score = Score(action, situation, profile) + (action == current ? _settings.CommitmentBonus : 0f);
                if (score > bestScore)
                {
                    best = action;
                    bestScore = score;
                }
            }

            return best;
        }

        private float CollectScore(BotSituation situation)
        {
            var free = 1f - situation.LoadShare;
            return free <= 0f ? 0f : _settings.CollectWeight * free * (_settings.CollectFloor + (1f - _settings.CollectFloor) * situation.SnowRichness);
        }

        private float DeliverScore(BotSituation situation, BotProfile profile)
        {
            if (situation.Load <= 0)
            {
                return 0f;
            }

            if (situation.PlayingRemaining <= situation.DropOffTravelTime + _settings.DeliveryMargin)
            {
                return _settings.DeliverWeight * _settings.UrgentDeliveryBoost;
            }

            var share = situation.LoadShare;
            var score = profile.DeliverEagerness + share * Mathf.Sqrt(share);
            if (situation.Load >= situation.Capacity)
            {
                score += _settings.FullBucketBonus;
            }

            if (IsWorthToppingUp(situation))
            {
                score *= 1f - _settings.GreedDiscount * profile.Greed;
            }

            var nearness = 1f - Mathf.Clamp01(situation.DropOffTravelTime / _settings.NearDropOffTravelTime);
            return _settings.DeliverWeight * score * (1f + _settings.NearDropOffBonus * nearness);
        }

        private bool IsWorthToppingUp(BotSituation situation)
        {
            var missing = situation.NextTierLoad - situation.Load;
            return missing > 0 && missing <= _settings.GreedReach && situation.SnowRichness > _settings.GreedRichnessMinimum;
        }

        private float PileScore(BotSituation situation)
        {
            var share = situation.LoadShare;
            if (!situation.HasPile || share >= _settings.PileFullShare)
            {
                return 0f;
            }

            var scarcity = _settings.PileScarcityBase + (1f - _settings.PileScarcityBase) * (1f - situation.SnowRichness);
            return _settings.PileWeight * (1f - share) * scarcity / (1f + situation.PileDistance / _settings.PileFalloff);
        }

        private float RamScore(BotSituation situation, BotProfile profile)
        {
            if (!situation.HasRamTarget || situation.RamTargetLoad < _settings.RamMinVictimLoad || situation.Capacity <= 0)
            {
                return 0f;
            }

            var prize = (float)situation.RamTargetLoad / situation.Capacity;
            return _settings.RamWeight * profile.Aggression * prize * (_settings.RamLoadOffset - situation.LoadShare)
                / (1f + situation.RamTargetDistance / _settings.RamFalloff);
        }

        private float EvadeScore(BotSituation situation, BotProfile profile)
        {
            if (situation.ThreatLevel <= 0f || situation.Load <= 0)
            {
                return 0f;
            }

            return _settings.EvadeWeight * profile.Caution * situation.ThreatLevel * (_settings.EvadeBase + situation.LoadShare);
        }
    }
}
