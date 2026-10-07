using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class MatchResponse
    {
        [JsonProperty("match_id")] public string MatchId { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        [JsonProperty("rejection_reason")] public string RejectionReason { get; set; }
    }
}
