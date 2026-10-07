using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotUtility
    {
        private const int ActionCount = (int)BotAction.Evade + 1;
        private const float DeliverFloor = 0.15f;
        private const float FullBucketBonus = 0.3f;
        private const float UrgentDelivery = 2f;
        private const float GreedRichnessMinimum = 0.25f;
        private const float GreedDiscount = 0.8f;
        private const float NearDropOffBonus = 0.3f;
        private const float NearDropOffTravelTime = 6f;
        private const float CollectFloor = 0.3f;
        private const float PileFullShare = 0.9f;
        private const float PileBase = 0.6f;
        private const float RamLoadOffset = 1.3f;
        private const float EvadeBase = 0.4f;

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
            return free <= 0f ? 0f : _settings.CollectWeight * free * (CollectFloor + (1f - CollectFloor) * situation.SnowRichness);
        }

        private float DeliverScore(BotSituation situation, BotProfile profile)
        {
            if (situation.Load <= 0)
            {
                return 0f;
            }

            if (situation.PlayingRemaining <= situation.DropOffTravelTime + _settings.DeliveryMargin)
            {
                return _settings.DeliverWeight * UrgentDelivery;
            }

            var share = situation.LoadShare;
            var score = DeliverFloor + share * Mathf.Sqrt(share);
            if (situation.Load >= situation.Capacity)
            {
                score += FullBucketBonus;
            }

            if (IsWorthToppingUp(situation))
            {
                score *= 1f - GreedDiscount * profile.Greed;
            }

            var nearness = 1f - Mathf.Clamp01(situation.DropOffTravelTime / NearDropOffTravelTime);
            return _settings.DeliverWeight * score * (1f + NearDropOffBonus * nearness);
        }

        private bool IsWorthToppingUp(BotSituation situation)
        {
            var missing = situation.NextTierLoad - situation.Load;
            return missing > 0 && missing <= _settings.GreedReach && situation.SnowRichness > GreedRichnessMinimum;
        }

        private float PileScore(BotSituation situation)
        {
            var share = situation.LoadShare;
            if (!situation.HasPile || share >= PileFullShare)
            {
                return 0f;
            }

            var scarcity = PileBase + (1f - PileBase) * (1f - situation.SnowRichness);
            return _settings.PileWeight * (1f - share) * scarcity / (1f + situation.PileDistance / _settings.PileFalloff);
        }

        private float RamScore(BotSituation situation, BotProfile profile)
        {
            if (!situation.HasRamTarget || situation.RamTargetLoad < _settings.RamMinVictimLoad || situation.Capacity <= 0)
            {
                return 0f;
            }

            var prize = (float)situation.RamTargetLoad / situation.Capacity;
            return _settings.RamWeight * profile.Aggression * prize * (RamLoadOffset - situation.LoadShare)
                / (1f + situation.RamTargetDistance / _settings.RamFalloff);
        }

        private float EvadeScore(BotSituation situation, BotProfile profile)
        {
            if (situation.ThreatLevel <= 0f || situation.Load <= 0)
            {
                return 0f;
            }

            return _settings.EvadeWeight * profile.Caution * situation.ThreatLevel * (EvadeBase + situation.LoadShare);
        }
    }
}
