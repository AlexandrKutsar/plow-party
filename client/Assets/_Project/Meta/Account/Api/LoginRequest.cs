using Newtonsoft.Json;

namespace PlowParty.Meta.Account.Api
{
    public sealed class LoginRequest
    {
        [JsonProperty("device_id")] public string DeviceId { get; set; }
    }
}
