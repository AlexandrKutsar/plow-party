using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class RosterSlotRequest
    {
        [JsonProperty("slot")] public int Slot { get; set; }

        [JsonProperty("account_id")] public string AccountId { get; set; }
    }
}
