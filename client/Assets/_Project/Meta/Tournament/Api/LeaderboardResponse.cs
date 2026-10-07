using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class LeaderboardResponse
    {
        [JsonProperty("day")] public string Day { get; set; }

        [JsonProperty("final")] public bool Final { get; set; }

        [JsonProperty("players")] public int Players { get; set; }

        [JsonProperty("me")] public StandingResponse Me { get; set; }

        [JsonProperty("entries")] public List<StandingResponse> Entries { get; set; } = new List<StandingResponse>();
    }
}
