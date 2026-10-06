using NUnit.Framework;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Tests
{
    public sealed class VehicleArenaTests
    {
        [Test]
        public void Contains_PointInsideBox_ReturnsTrue()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(1f, 1f), new Vector2(0.5f, 0.25f));

            Assert.That(arena.Contains(new Vector2(1.4f, 1.2f)), Is.True);
        }

        [Test]
        public void Contains_PointOutsideBoxAlongShortAxis_ReturnsFalse()
        {
            var arena = VehicleArena.Create().AddBox(new Vector2(1f, 1f), new Vector2(0.5f, 0.25f));

            Assert.That(arena.Contains(new Vector2(1f, 1.3f)), Is.False);
        }

        [Test]
        public void Contains_PointInsideCircle_ReturnsTrue()
        {
            var arena = VehicleArena.Create().AddCircle(new Vector2(-2f, 0f), 1f);

            Assert.That(arena.Contains(new Vector2(-2.6f, 0.6f)), Is.True);
        }

        [Test]
        public void Contains_PointInCircleBoundingBoxCorner_ReturnsFalse()
        {
            var arena = VehicleArena.Create().AddCircle(new Vector2(-2f, 0f), 1f);

            Assert.That(arena.Contains(new Vector2(-2.8f, 0.8f)), Is.False);
        }

        [Test]
        public void Contains_EmptyArena_ReturnsFalse()
        {
            Assert.That(VehicleArena.Create().Contains(Vector2.zero), Is.False);
        }
    }
}
