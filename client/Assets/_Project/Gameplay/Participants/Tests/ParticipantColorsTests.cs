using System;
using System.Collections.Generic;
using NUnit.Framework;
using PlowParty.Gameplay.Participants.Simulation;

namespace PlowParty.Gameplay.Participants.Tests
{
    public sealed class ParticipantColorsTests
    {
        private const int PaletteSize = 8;

        [Test]
        public void Assign_NoPreference_GivesTheOnlyFreeColor()
        {
            var taken = new List<int> { 0, 1, 2, 3, 5, 6, 7 };

            for (var seed = 0; seed < 20; seed++)
            {
                Assert.That(ParticipantColors.Assign(ParticipantColors.NoPreference, taken, PaletteSize, new Random(seed)), Is.EqualTo(4));
            }
        }

        [Test]
        public void Assign_NoPreference_GivesRandomFreeColors()
        {
            var given = new HashSet<int>();

            for (var seed = 0; seed < 40; seed++)
            {
                given.Add(ParticipantColors.Assign(ParticipantColors.NoPreference, new List<int> { 2 }, PaletteSize, new Random(seed)));
            }

            Assert.That(given, Has.No.Member(2));
            Assert.That(given.Count, Is.GreaterThan(3));
        }

        [Test]
        public void Assign_SixParticipantsWithoutPreference_EveryColorIsUnique()
        {
            var random = new Random(7);
            var taken = new List<int>();

            for (var slot = 0; slot < 6; slot++)
            {
                var color = ParticipantColors.Assign(ParticipantColors.NoPreference, taken, PaletteSize, random);
                Assert.That(taken, Has.No.Member(color));
                taken.Add(color);
            }
        }

        [Test]
        public void Assign_FreePick_IsHonoured()
        {
            var taken = new List<int> { 0, 1, 5 };

            for (var seed = 0; seed < 20; seed++)
            {
                Assert.That(ParticipantColors.Assign(3, taken, PaletteSize, new Random(seed)), Is.EqualTo(3));
            }
        }

        [Test]
        public void Assign_TakenPreference_GivesTheNearestFreeColor()
        {
            Assert.That(ParticipantColors.Assign(3, new List<int> { 3, 4 }, PaletteSize, new Random(1)), Is.EqualTo(2));
        }

        [Test]
        public void Assign_TakenPreferenceWithTwoNearest_GivesTheNextOne()
        {
            Assert.That(ParticipantColors.Assign(3, new List<int> { 3 }, PaletteSize, new Random(1)), Is.EqualTo(4));
        }

        [Test]
        public void Assign_TakenPreferenceAtPaletteEnd_WrapsAround()
        {
            Assert.That(ParticipantColors.Assign(7, new List<int> { 7, 6 }, PaletteSize, new Random(1)), Is.EqualTo(0));
        }

        [Test]
        public void Assign_PreferenceOutsidePalette_GivesAFreeColor()
        {
            var taken = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

            Assert.That(ParticipantColors.Assign(PaletteSize + 3, taken, PaletteSize, new Random(1)), Is.EqualTo(7));
        }

        [Test]
        public void Assign_EveryColorTaken_StillGivesAPaletteColor()
        {
            var taken = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

            var color = ParticipantColors.Assign(ParticipantColors.NoPreference, taken, PaletteSize, new Random(3));

            Assert.That(color, Is.InRange(0, PaletteSize - 1));
        }
    }
}
