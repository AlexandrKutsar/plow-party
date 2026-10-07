using Newtonsoft.Json;

namespace PlowParty.Meta.Account.Api
{
    public sealed class AccountResponse
    {
        [JsonProperty("account_id")] public string AccountId { get; set; }

        [JsonProperty("nickname")] public string Nickname { get; set; }
    }
}
