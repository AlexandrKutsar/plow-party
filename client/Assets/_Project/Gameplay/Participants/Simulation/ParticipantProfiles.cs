using System;
using System.Collections.Generic;

namespace PlowParty.Gameplay.Participants.Simulation
{
    public static class ParticipantProfiles
    {
        public const int MaxNicknameLength = 16;
        public const int SpeciesCount = (int)CritterSpecies.Beaver + 1;

        public static string FallbackNickname(int slot)
        {
            return $"Player {slot + 1}";
        }

        public static string PlayerNickname(string tokenNickname, int slot)
        {
            var nickname = tokenNickname?.Trim();
            return string.IsNullOrEmpty(nickname) ? FallbackNickname(slot) : Shorten(nickname);
        }

        public static string BotNickname(IReadOnlyList<string> pool, IReadOnlyCollection<string> taken, int slot, Random random)
        {
            if (pool.Count == 0)
            {
                return FallbackNickname(slot);
            }

            var free = new List<string>(pool.Count);
            foreach (var nickname in pool)
            {
                if (!Contains(taken, nickname))
                {
                    free.Add(nickname);
                }
            }

            if (free.Count > 0)
            {
                return Shorten(free[random.Next(free.Count)]);
            }

            var basis = pool[random.Next(pool.Count)];
            for (var number = 2; ; number++)
            {
                var suffix = " " + number;
                var candidate = Shorten(basis, MaxNicknameLength - suffix.Length) + suffix;
                if (!Contains(taken, candidate))
                {
                    return candidate;
                }
            }
        }

        public static CritterSpecies PickSpecies(IReadOnlyCollection<CritterSpecies> taken, Random random)
        {
            var free = new List<CritterSpecies>(SpeciesCount);
            for (var species = 0; species < SpeciesCount; species++)
            {
                if (!Contains(taken, (CritterSpecies)species))
                {
                    free.Add((CritterSpecies)species);
                }
            }

            return free.Count > 0 ? free[random.Next(free.Count)] : (CritterSpecies)random.Next(SpeciesCount);
        }

        private static string Shorten(string nickname, int maxLength = MaxNicknameLength)
        {
            return nickname.Length > maxLength ? nickname.Substring(0, maxLength) : nickname;
        }

        private static bool Contains<T>(IReadOnlyCollection<T> collection, T value)
        {
            foreach (var item in collection)
            {
                if (EqualityComparer<T>.Default.Equals(item, value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
