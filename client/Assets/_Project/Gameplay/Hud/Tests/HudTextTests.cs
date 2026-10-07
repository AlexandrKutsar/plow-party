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
        public void ParticipantName_LocalAndRemote_YouOrSlotNumber()
        {
            Assert.That(HudText.ParticipantName(0, true), Is.EqualTo("You"));
            Assert.That(HudText.ParticipantName(2, false), Is.EqualTo("Player 3"));
        }

        [Test]
        public void Place_Numbers_UseEnglishOrdinals()
        {
            Assert.That(HudText.Place(1), Is.EqualTo("1st"));
            Assert.That(HudText.Place(2), Is.EqualTo("2nd"));
            Assert.That(HudText.Place(3), Is.EqualTo("3rd"));
            Assert.That(HudText.Place(4), Is.EqualTo("4th"));
            Assert.That(HudText.Place(11), Is.EqualTo("11th"));
        }
    }
}
