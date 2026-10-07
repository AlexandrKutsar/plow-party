using System;

namespace PlowParty.Meta.Session.Simulation
{
    public static class RoomCode
    {
        public const int Length = 5;
        public const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        public static string Generate(Random random)
        {
            var symbols = new char[Length];
            for (var i = 0; i < Length; i++)
            {
                symbols[i] = Alphabet[random.Next(Alphabet.Length)];
            }

            return new string(symbols);
        }

        public static bool TryParse(string input, out string code)
        {
            code = (input ?? string.Empty).Trim().ToUpperInvariant();
            if (code.Length != Length)
            {
                return false;
            }

            foreach (var symbol in code)
            {
                if (Alphabet.IndexOf(symbol) < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
