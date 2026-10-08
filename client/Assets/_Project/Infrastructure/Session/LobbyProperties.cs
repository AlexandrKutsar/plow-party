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

        public static Dictionary<string, SessionProperty> Hidden(string pool)
        {
            return Open(pool, 0L, 0L);
        }

        public static Dictionary<string, SessionProperty> Open(string pool, long startsAtMilliseconds, long openedAtMilliseconds)
        {
            return new Dictionary<string, SessionProperty>
            {
                [Pool] = pool,
                [StartsAt] = Format(startsAtMilliseconds),
                [OpenedAt] = Format(openedAtMilliseconds),
            };
        }

        public static bool IsInPool(IReadOnlyDictionary<string, SessionProperty> properties, string pool)
        {
            return properties != null && properties.TryGetValue(Pool, out var value) && value.IsString && (string)value.PropertyValue == pool;
        }

        public static long ReadMilliseconds(IReadOnlyDictionary<string, SessionProperty> properties, string key)
        {
            if (properties == null || !properties.TryGetValue(key, out var value) || !value.IsString)
            {
                return 0L;
            }

            return long.TryParse((string)value.PropertyValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds) ? milliseconds : 0L;
        }

        private static string Format(long milliseconds)
        {
            return milliseconds.ToString(CultureInfo.InvariantCulture);
        }
    }
}
