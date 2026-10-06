using NUnit.Framework;
using PlowParty.Gameplay.Hud.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Hud.Tests
{
    public sealed class DropOffArrowTests
    {
        private static readonly Rect Screen = new Rect(0f, 0f, 1000f, 500f);
        private const float Margin = 50f;

        [Test]
        public void TryPlace_TargetOnScreen_IsHidden()
        {
            Assert.That(DropOffArrow.TryPlace(new Vector3(500f, 250f, 10f), Screen, Margin, out _, out _), Is.False);
        }

        [Test]
        public void TryPlace_TargetInsideMarginBand_IsShown()
        {
            Assert.That(DropOffArrow.TryPlace(new Vector3(980f, 250f, 10f), Screen, Margin, out _, out _), Is.True);
        }

        [Test]
        public void TryPlace_TargetRightOfScreen_PinsToRightEdgePointingRight()
        {
            DropOffArrow.TryPlace(new Vector3(3000f, 250f, 10f), Screen, Margin, out var position, out var angle);

            Assert.That(position.x, Is.EqualTo(950f).Within(1e-3f));
            Assert.That(position.y, Is.EqualTo(250f).Within(1e-3f));
            Assert.That(angle, Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void TryPlace_TargetAboveScreen_PinsToTopEdgePointingUp()
        {
            DropOffArrow.TryPlace(new Vector3(500f, 2000f, 10f), Screen, Margin, out var position, out var angle);

            Assert.That(position.y, Is.EqualTo(450f).Within(1e-3f));
            Assert.That(angle, Is.EqualTo(90f).Within(1e-3f));
        }

        [Test]
        public void TryPlace_TargetDiagonal_StaysInsideMarginRect()
        {
            DropOffArrow.TryPlace(new Vector3(-5000f, -5000f, 10f), Screen, Margin, out var position, out _);

            Assert.That(position.x, Is.InRange(Margin - 1e-3f, Screen.width - Margin + 1e-3f));
            Assert.That(position.y, Is.EqualTo(Margin).Within(1e-3f));
        }

        [Test]
        public void TryPlace_TargetBehindCamera_PointsAwayFromProjectedPoint()
        {
            DropOffArrow.TryPlace(new Vector3(600f, 250f, -10f), Screen, Margin, out var position, out var angle);

            Assert.That(position.x, Is.EqualTo(Margin).Within(1e-3f));
            Assert.That(Mathf.Abs(angle), Is.EqualTo(180f).Within(1e-3f));
        }

        [Test]
        public void TryPlace_OffsetSafeArea_PinsInsideSafeArea()
        {
            var safeArea = new Rect(100f, 0f, 800f, 500f);

            DropOffArrow.TryPlace(new Vector3(-3000f, 250f, 10f), safeArea, Margin, out var position, out _);

            Assert.That(position.x, Is.EqualTo(150f).Within(1e-3f));
            Assert.That(position.y, Is.EqualTo(250f).Within(1e-3f));
        }

        [Test]
        public void TryPlace_TargetUnderNotchOutsideSafeArea_IsShown()
        {
            var safeArea = new Rect(100f, 0f, 800f, 500f);

            Assert.That(DropOffArrow.TryPlace(new Vector3(60f, 250f, 10f), safeArea, Margin, out _, out _), Is.True);
        }
    }
}
