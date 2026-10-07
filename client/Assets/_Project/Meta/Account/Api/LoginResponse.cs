using Newtonsoft.Json;

namespace PlowParty.Meta.Account.Api
{
    public sealed class LoginResponse
    {
        [JsonProperty("account_id")] public string AccountId { get; set; }

        [JsonProperty("nickname")] public string Nickname { get; set; }

        [JsonProperty("token")] public string Token { get; set; }
    }
}
