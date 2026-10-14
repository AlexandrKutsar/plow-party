using NUnit.Framework;
using PlowParty.Meta.Lobby.Simulation;

namespace PlowParty.Meta.Lobby.Tests
{
    public sealed class PodiumLayoutTests
    {
        [Test]
        public void Offset_SoloVehicle_StandsInTheCentre()
        {
            Assert.That(PodiumLayout.Offset(0, 1, 2f), Is.EqualTo(0f));
        }

        [TestCase(0, -1f)]
        [TestCase(1, 1f)]
        public void Offset_TwoVehicles_StandAroundTheCentre(int position, float expected)
        {
            Assert.That(PodiumLayout.Offset(position, 2, 2f), Is.EqualTo(expected));
        }

        [TestCase(0, -5f)]
        [TestCase(2, -1f)]
        [TestCase(5, 5f)]
        public void Offset_SixVehicles_FillTheRowLeftToRight(int position, float expected)
        {
            Assert.That(PodiumLayout.Offset(position, 6, 2f), Is.EqualTo(expected));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Scale_RowFitsTheStage_KeepsFullSize(int count)
        {
            Assert.That(PodiumLayout.Scale(count, 2f, 5f), Is.EqualTo(1f));
        }

        [Test]
        public void Scale_RowWiderThanTheStage_ShrinksToFit()
        {
            Assert.That(PodiumLayout.Scale(5, 2f, 5f), Is.EqualTo(0.5f));
        }
    }
}
