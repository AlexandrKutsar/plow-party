using NUnit.Framework;
using PlowParty.Gameplay.Bots.Simulation;

namespace PlowParty.Gameplay.Bots.Tests
{
    public sealed class BotDifficultyRulesTests
    {
        [Test]
        public void Choose_NoStrongBotYet_IsStrong()
        {
            Assert.That(BotDifficultyRules.Choose(false, 0.1, 0.5f), Is.EqualTo(BotDifficulty.Strong));
            Assert.That(BotDifficultyRules.Choose(false, 0.9, 0.5f), Is.EqualTo(BotDifficulty.Strong));
        }

        [Test]
        public void Choose_StrongBotSeated_RollBelowWeakShareIsWeak()
        {
            Assert.That(BotDifficultyRules.Choose(true, 0.2, 0.5f), Is.EqualTo(BotDifficulty.Weak));
        }

        [Test]
        public void Choose_StrongBotSeated_RollAboveWeakShareIsMedium()
        {
            Assert.That(BotDifficultyRules.Choose(true, 0.7, 0.5f), Is.EqualTo(BotDifficulty.Medium));
        }
    }
}
