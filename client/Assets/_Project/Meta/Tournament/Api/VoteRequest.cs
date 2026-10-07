using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class VoteRequest
    {
        [JsonProperty("scores")] public List<SlotScore> Scores { get; set; } = new List<SlotScore>();

        [JsonProperty("interrupted_at_seconds")] public int? InterruptedAtSeconds { get; set; }
    }
}
