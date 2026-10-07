using System.Text;

namespace PlowParty.Meta.Account.Simulation
{
    public static class NicknameRules
    {
        public const int MinLength = 3;
        public const int MaxLength = 16;
        public const string LengthError = "Ник — от 3 до 16 символов";
        public const string SymbolError = "Только буквы, цифры, пробел, _ и -";
        public const string SpacesError = "Без двойных пробелов";

        public static NicknameCheck Check(string raw)
        {
            var nickname = (raw ?? string.Empty).Normalize(NormalizationForm.FormC).Trim();
            if (nickname.Length < MinLength || nickname.Length > MaxLength)
            {
                return NicknameCheck.Invalid(LengthError);
            }

            if (!HasOnlyAllowedSymbols(nickname))
            {
                return NicknameCheck.Invalid(SymbolError);
            }

            return nickname.Contains("  ") ? NicknameCheck.Invalid(SpacesError) : NicknameCheck.Valid(nickname);
        }

        private static bool HasOnlyAllowedSymbols(string nickname)
        {
            foreach (var symbol in nickname)
            {
                var allowed = char.IsLetter(symbol) || (symbol >= '0' && symbol <= '9') || symbol == ' ' || symbol == '_' || symbol == '-';
                if (!allowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
