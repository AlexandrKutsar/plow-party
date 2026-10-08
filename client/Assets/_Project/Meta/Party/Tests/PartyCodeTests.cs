using System;
using NUnit.Framework;
using PlowParty.Meta.Party.Simulation;

namespace PlowParty.Meta.Party.Tests
{
    public sealed class PartyCodeTests
    {
        [Test]
        public void Generate_AnySeed_HasFiveSymbolsFromAlphabet()
        {
            var random = new Random(7);
            for (var i = 0; i < 200; i++)
            {
                var code = PartyCode.Generate(random);

                Assert.That(code.Length, Is.EqualTo(PartyCode.Length));
                foreach (var symbol in code)
                {
                    Assert.That(PartyCode.Alphabet, Does.Contain(symbol.ToString()));
                }
            }
        }

        [Test]
        public void Generate_SameSeed_SameCode()
        {
            Assert.That(PartyCode.Generate(new Random(42)), Is.EqualTo(PartyCode.Generate(new Random(42))));
        }

        [TestCase('I')]
        [TestCase('L')]
        [TestCase('O')]
        [TestCase('0')]
        [TestCase('1')]
        public void Alphabet_AmbiguousSymbol_IsExcluded(char symbol)
        {
            Assert.That(PartyCode.Alphabet, Does.Not.Contain(symbol.ToString()));
        }

        [Test]
        public void TryParse_LowerCaseWithSpaces_ReturnsUpperCaseCode()
        {
            Assert.That(PartyCode.TryParse("  ab3xz ", out var code), Is.True);
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
            Assert.That(PartyCode.TryParse(input, out _), Is.False);
        }

        [Test]
        public void SessionName_PoolAndCode_NamesThePartySessionInsideThePool()
        {
            Assert.That(PartyCode.SessionName("dev-0.1", "AB3XZ"), Is.EqualTo("dev-0.1-party-AB3XZ"));
        }
    }
}
