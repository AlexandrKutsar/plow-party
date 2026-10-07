using System;
using NUnit.Framework;
using PlowParty.Meta.Session.Simulation;

namespace PlowParty.Meta.Session.Tests
{
    public sealed class RoomCodeTests
    {
        [Test]
        public void Generate_AnySeed_HasFiveSymbolsFromAlphabet()
        {
            var random = new Random(7);
            for (var i = 0; i < 200; i++)
            {
                var code = RoomCode.Generate(random);

                Assert.That(code.Length, Is.EqualTo(RoomCode.Length));
                foreach (var symbol in code)
                {
                    Assert.That(RoomCode.Alphabet, Does.Contain(symbol.ToString()));
                }
            }
        }

        [Test]
        public void Generate_SameSeed_SameCode()
        {
            Assert.That(RoomCode.Generate(new Random(42)), Is.EqualTo(RoomCode.Generate(new Random(42))));
        }

        [TestCase('I')]
        [TestCase('L')]
        [TestCase('O')]
        [TestCase('0')]
        [TestCase('1')]
        public void Alphabet_AmbiguousSymbol_IsExcluded(char symbol)
        {
            Assert.That(RoomCode.Alphabet, Does.Not.Contain(symbol.ToString()));
        }

        [Test]
        public void TryParse_LowerCaseWithSpaces_ReturnsUpperCaseCode()
        {
            Assert.That(RoomCode.TryParse("  ab3xz ", out var code), Is.True);
            Assert.That(code, Is.EqualTo("AB3XZ"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("ABCD")]
        [TestCase("ABCDEF")]
        [TestCase("ABCD0")]
        [TestCase("ABCDI")]
        [TestCase("AB-CD")]
        public void TryParse_Malformed_ReturnsFalse(string input)
        {
            Assert.That(RoomCode.TryParse(input, out _), Is.False);
        }
    }
}
