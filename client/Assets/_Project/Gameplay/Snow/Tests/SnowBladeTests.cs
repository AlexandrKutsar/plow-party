using NUnit.Framework;
using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    public sealed class SnowBladeTests
    {
        [Test]
        public void Ahead_VehicleFacingEast_CentresBladeOffsetAlongFacingWithConfiguredSize()
        {
            var blade = SnowBlade.Ahead(new Vector2(1f, 2f), Vector2.right, SnowTestSettings.Create());

            Assert.That(blade.Centre.x, Is.EqualTo(1.75f).Within(1e-5f));
            Assert.That(blade.Centre.y, Is.EqualTo(2f).Within(1e-5f));
            Assert.That(blade.Forward, Is.EqualTo(Vector2.right));
            Assert.That(blade.Width, Is.EqualTo(1f));
            Assert.That(blade.Depth, Is.EqualTo(0.4f));
        }

        [Test]
        public void Ahead_UnnormalisedFacing_OffsetsByConfiguredDistance()
        {
            var blade = SnowBlade.Ahead(Vector2.zero, new Vector2(0f, 4f), SnowTestSettings.Create());

            Assert.That(blade.Centre.y, Is.EqualTo(0.75f).Within(1e-5f));
        }
    }
}
