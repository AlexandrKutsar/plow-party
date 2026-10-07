using Newtonsoft.Json;

namespace PlowParty.Meta.Tournament.Api
{
    public sealed class SlotScore
    {
        [JsonProperty("slot")] public int Slot { get; set; }

        [JsonProperty("score")] public int Score { get; set; }
    }
}
