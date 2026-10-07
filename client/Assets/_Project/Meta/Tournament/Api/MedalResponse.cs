using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class MedalResponse
    {
        [JsonProperty("account_id")] public string AccountId { get; set; }

        [JsonProperty("medal")] public string Medal { get; set; }
    }
}
