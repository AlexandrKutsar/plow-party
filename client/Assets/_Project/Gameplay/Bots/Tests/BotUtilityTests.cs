using NUnit.Framework;
using PlowParty.Gameplay.Bots.Simulation;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class BotUtilityTests
    {
        private const double NoMistake = 1.0;

        private BotUtility _utility;
        private BotProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _utility = new BotUtility(BotTestSettings.Create());
            _profile = BotTestSettings.Profile();
        }

        [Test]
        public void Choose_FullBucket_Delivers()
        {
            var situation = Situation(100, 0, 1f);

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Choose_EmptyBucketAndSnowAround_Collects()
        {
            var situation = Situation(0, 51, 1f);

            Assert.That(_utility.Choose(situation, _profile, BotAction.Deliver, NoMistake), Is.EqualTo(BotAction.Collect));
        }

        [Test]
        public void Choose_JustBelowAMultiplierTier_GreedyBotTopsUp()
        {
            var situation = Situation(45, 51, 0.5f);

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Collect));
        }

        [Test]
        public void Choose_JustBelowAMultiplierTier_ModestBotDelivers()
        {
            var situation = Situation(45, 51, 0.5f);
            _profile.Greed = 0f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Choose_PastATierWithTheNextFarAway_Delivers()
        {
            var situation = Situation(60, 100, 0.8f);

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Choose_MatchEndingWithSomeLoad_Delivers()
        {
            var situation = Situation(20, 51, 1f);
            situation.PlayingRemaining = 5f;
            situation.DropOffTravelTime = 3f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Choose_LoadedRivalClose_AggressiveBotRams()
        {
            var situation = Situation(0, 51, 0.3f);
            situation.HasRamTarget = true;
            situation.RamTargetLoad = 80;
            situation.RamTargetDistance = 3f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Ram));
        }

        [Test]
        public void Choose_LoadedRivalClose_PeacefulBotKeepsCollecting()
        {
            var situation = Situation(0, 51, 0.3f);
            situation.HasRamTarget = true;
            situation.RamTargetLoad = 80;
            situation.RamTargetDistance = 3f;
            _profile.Aggression = 0.1f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Collect));
        }

        [Test]
        public void Choose_IncomingRammerWhileLoaded_Evades()
        {
            var situation = Situation(70, 100, 0.5f);
            situation.ThreatLevel = 0.9f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Evade));
        }

        [Test]
        public void Choose_IncomingRammerWithEmptyBucket_IgnoresIt()
        {
            var situation = Situation(0, 51, 1f);
            situation.ThreatLevel = 1f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Collect));
        }

        [Test]
        public void Choose_PileCloseAndSnowScarce_ChasesThePile()
        {
            var situation = Situation(10, 51, 0.2f);
            situation.HasPile = true;
            situation.PileDistance = 3f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.ChasePile));
        }

        [Test]
        public void Choose_MistakeRollUnderChance_PicksTheSecondBest()
        {
            var situation = Situation(45, 51, 0.8f);
            _profile.MistakeChance = 0.5f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, 0.1), Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Choose_MistakeWithNoRealSecondChoice_StillPicksTheBest()
        {
            var situation = Situation(100, 0, 1f);
            _profile.MistakeChance = 1f;

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, 0.0), Is.EqualTo(BotAction.Deliver));
        }

        [Test]
        public void Choose_TwoActionsAlmostEqual_KeepsTheCurrentOne()
        {
            var situation = Situation(30, 51, 0.3f);
            situation.HasPile = true;
            situation.PileDistance = 10f;
            var collect = _utility.Score(BotAction.Collect, situation, _profile);
            var pile = _utility.Score(BotAction.ChasePile, situation, _profile);
            Assume.That(System.Math.Abs(collect - pile), Is.LessThan(0.15f));

            Assert.That(_utility.Choose(situation, _profile, BotAction.Collect, NoMistake), Is.EqualTo(BotAction.Collect));
            Assert.That(_utility.Choose(situation, _profile, BotAction.ChasePile, NoMistake), Is.EqualTo(BotAction.ChasePile));
        }

        private static BotSituation Situation(int load, int nextTier, float richness)
        {
            return new BotSituation
            {
                Load = load,
                Capacity = 100,
                NextTierLoad = nextTier,
                SnowRichness = richness,
                DropOffTravelTime = 2f,
                PlayingRemaining = 120f,
            };
        }
    }
}
