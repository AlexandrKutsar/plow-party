using NUnit.Framework;
using PlowParty.Gameplay.Hud.Simulation;

namespace PlowParty.Gameplay.Hud.Tests
{
    public sealed class HudTextTests
    {
        [Test]
        public void WholeSecondsLeft_Fraction_RoundsUp()
        {
            Assert.That(HudText.WholeSecondsLeft(2.2f), Is.EqualTo(3));
            Assert.That(HudText.WholeSecondsLeft(0.01f), Is.EqualTo(1));
        }

        [Test]
        public void WholeSecondsLeft_ZeroOrNegative_IsZero()
        {
            Assert.That(HudText.WholeSecondsLeft(0f), Is.EqualTo(0));
            Assert.That(HudText.WholeSecondsLeft(-1f), Is.EqualTo(0));
        }

        [Test]
        public void WholeSecondsLeft_WholeNumber_StaysThatNumber()
        {
            Assert.That(HudText.WholeSecondsLeft(180f), Is.EqualTo(180));
        }

        [Test]
        public void Clock_Seconds_FormatsMinutesAndPaddedSeconds()
        {
            Assert.That(HudText.Clock(180), Is.EqualTo("3:00"));
            Assert.That(HudText.Clock(65), Is.EqualTo("1:05"));
            Assert.That(HudText.Clock(9), Is.EqualTo("0:09"));
        }

        [Test]
        public void Multiplier_Values_DropTrailingZero()
        {
            Assert.That(HudText.Multiplier(1f), Is.EqualTo("×1"));
            Assert.That(HudText.Multiplier(1.5f), Is.EqualTo("×1.5"));
            Assert.That(HudText.Multiplier(2f), Is.EqualTo("×2"));
        }

        [Test]
        public void WaitingForPlayers_SeatedAndSlots_ShowsTheirCount()
        {
            Assert.That(HudText.WaitingForPlayers(3, 6), Is.EqualTo("Матч скоро начнётся. Ожидание игроков 3/6"));
        }

        [Test]
        public void NextMatchIn_Seconds_NamesThem()
        {
            Assert.That(HudText.NextMatchIn(7), Is.EqualTo("Следующий матч через 7"));
        }

        [Test]
        public void BlizzardIn_Seconds_NamesThem()
        {
            Assert.That(HudText.BlizzardIn(3), Is.EqualTo("Метель через 3!"));
        }

        [Test]
        public void ScoreGain_Points_HasAPlusSign()
        {
            Assert.That(HudText.ScoreGain(12), Is.EqualTo("+12"));
        }

        [Test]
        public void Place_Numbers_UseRussianOrdinals()
        {
            Assert.That(HudText.Place(1), Is.EqualTo("1-е"));
            Assert.That(HudText.Place(3), Is.EqualTo("3-е"));
            Assert.That(HudText.Place(11), Is.EqualTo("11-е"));
        }
    }
}
