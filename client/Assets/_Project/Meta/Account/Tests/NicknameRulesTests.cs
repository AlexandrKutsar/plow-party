using NUnit.Framework;
using PlowParty.Meta.Account.Simulation;

namespace PlowParty.Meta.Account.Tests
{
    public sealed class NicknameRulesTests
    {
        [TestCase("Plower0042")]
        [TestCase("Лиса_Алиса")]
        [TestCase("big bear-7")]
        [TestCase("abc")]
        [TestCase("abcdefghijklmnop")]
        public void Check_AllowedNickname_IsValid(string nickname)
        {
            var check = NicknameRules.Check(nickname);

            Assert.That(check.IsValid, Is.True);
            Assert.That(check.Nickname, Is.EqualTo(nickname));
        }

        [Test]
        public void Check_SurroundingSpaces_AreTrimmed()
        {
            var check = NicknameRules.Check("  Fox  ");

            Assert.That(check.IsValid, Is.True);
            Assert.That(check.Nickname, Is.EqualTo("Fox"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("ab")]
        [TestCase("   ab   ")]
        public void Check_TooShort_IsInvalid(string nickname)
        {
            Assert.That(NicknameRules.Check(nickname).Error, Is.EqualTo(NicknameRules.LengthError));
        }

        [Test]
        public void Check_SeventeenCharacters_IsInvalid()
        {
            Assert.That(NicknameRules.Check("abcdefghijklmnopq").Error, Is.EqualTo(NicknameRules.LengthError));
        }

        [TestCase("fox!")]
        [TestCase("fox.bear")]
        [TestCase("fox\tbear")]
        [TestCase("fox٣bear")]
        public void Check_ForbiddenSymbol_IsInvalid(string nickname)
        {
            Assert.That(NicknameRules.Check(nickname).Error, Is.EqualTo(NicknameRules.SymbolError));
        }

        [Test]
        public void Check_ConsecutiveSpaces_IsInvalid()
        {
            Assert.That(NicknameRules.Check("fox  bear").Error, Is.EqualTo(NicknameRules.SpacesError));
        }

        [Test]
        public void Check_DecomposedLetters_AreComposedBeforeCounting()
        {
            var decomposed = "Ёж";

            var check = NicknameRules.Check(decomposed + "ик");

            Assert.That(check.IsValid, Is.True);
            Assert.That(check.Nickname, Is.EqualTo("Ёжик"));
        }
    }
}
