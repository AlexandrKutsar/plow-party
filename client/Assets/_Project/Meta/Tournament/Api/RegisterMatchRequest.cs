using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class RegisterMatchRequest
    {
        [JsonProperty("roster")] public List<RosterSlotRequest> Roster { get; set; } = new List<RosterSlotRequest>();
    }
}
