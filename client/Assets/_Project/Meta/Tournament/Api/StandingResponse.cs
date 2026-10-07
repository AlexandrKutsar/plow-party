using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class StandingResponse
    {
        [JsonProperty("rank")] public int Rank { get; set; }

        [JsonProperty("account_id")] public string AccountId { get; set; }

        [JsonProperty("nickname")] public string Nickname { get; set; }

        [JsonProperty("score")] public int Score { get; set; }
    }
}
