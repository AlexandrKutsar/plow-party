using System.Collections.Generic;
using System.Globalization;
using Fusion;

namespace PlowParty.Infrastructure.Session
{
    public static class LobbyProperties
    {
        public const string Pool = "pool";
        public const string StartsAt = "start";
        public const string OpenedAt = "opened";
        public const string Parties = "parties";

        public static Dictionary<string, SessionProperty> Hidden(string pool)
        {
            return Open(pool, 0L, 0L, string.Empty);
        }

        public static Dictionary<string, SessionProperty> Open(string pool, long startsAtMilliseconds, long openedAtMilliseconds, string parties)
        {
            return new Dictionary<string, SessionProperty>
            {
                [Pool] = pool,
                [StartsAt] = Format(startsAtMilliseconds),
                [OpenedAt] = Format(openedAtMilliseconds),
                [Parties] = parties ?? string.Empty,
            };
        }

        public static bool IsInPool(IReadOnlyDictionary<string, SessionProperty> properties, string pool)
        {
            return ReadString(properties, Pool) == pool;
        }

        public static long ReadMilliseconds(IReadOnlyDictionary<string, SessionProperty> properties, string key)
        {
            var text = ReadString(properties, key);
            return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds) ? milliseconds : 0L;
        }

        public static string ReadString(IReadOnlyDictionary<string, SessionProperty> properties, string key)
        {
            if (properties == null || !properties.TryGetValue(key, out var value) || !value.IsString)
            {
                return string.Empty;
            }

            return (string)value.PropertyValue;
        }

        private static string Format(long milliseconds)
        {
            return milliseconds.ToString(CultureInfo.InvariantCulture);
        }
    }
}
