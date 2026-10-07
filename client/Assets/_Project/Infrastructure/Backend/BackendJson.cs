using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PlowParty.Infrastructure.Backend
{
    public static class BackendJson
    {
        private const string ValueErrorPrefix = "Value error, ";

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public static string FirstValidationMessage(string json)
        {
            try
            {
                var detail = JObject.Parse(json)["detail"];
                var message = detail switch
                {
                    JArray errors when errors.Count > 0 => (string)errors[0]["msg"],
                    JValue value => (string)value,
                    _ => null,
                };
                return StripValueErrorPrefix(message ?? string.Empty);
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }

        private static string StripValueErrorPrefix(string message)
        {
            return message.StartsWith(ValueErrorPrefix) ? message.Substring(ValueErrorPrefix.Length) : message;
        }
    }
}
