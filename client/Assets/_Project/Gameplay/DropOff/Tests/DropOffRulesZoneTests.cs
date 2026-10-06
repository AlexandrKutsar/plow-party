using NUnit.Framework;
using PlowParty.Gameplay.DropOff.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.DropOff.Tests
{
    public sealed class DropOffRulesZoneTests
    {
        private static readonly Vector2 Centre = new Vector2(1f, -2f);

        private DropOffRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = DropOffTestSettings.CreateRules();
        }

        [Test]
        public void IsInZone_PointWithinRadius_IsTrue()
        {
            Assert.That(_rules.IsInZone(Centre, Centre + new Vector2(2f, 2f)), Is.True);
        }

        [Test]
        public void IsInZone_PointOnEdge_IsTrue()
        {
            Assert.That(_rules.IsInZone(Centre, Centre + new Vector2(0f, 3f)), Is.True);
        }

        [Test]
        public void IsInZone_PointBeyondRadius_IsFalse()
        {
            Assert.That(_rules.IsInZone(Centre, Centre + new Vector2(3.1f, 0f)), Is.False);
        }
    }
}
