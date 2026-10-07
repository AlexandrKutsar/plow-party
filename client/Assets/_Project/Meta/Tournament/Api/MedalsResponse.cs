using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class MedalsResponse
    {
        [JsonProperty("day")] public string Day { get; set; }

        [JsonProperty("medals")] public List<MedalResponse> Medals { get; set; } = new List<MedalResponse>();
    }
}
