using Newtonsoft.Json;

namespace PlowParty.Meta.Account
{
    public sealed class AccountRecord
    {
        [JsonProperty("device_id")] public string DeviceId { get; set; }

        [JsonProperty("account_id")] public string AccountId { get; set; }

        [JsonProperty("nickname")] public string Nickname { get; set; }

        [JsonProperty("token")] public string AuthToken { get; set; }
    }
}
