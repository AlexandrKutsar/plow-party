using Newtonsoft.Json;

namespace PlowParty.Meta.Account.Api
{
    public sealed class RenameRequest
    {
        [JsonProperty("nickname")] public string Nickname { get; set; }
    }
}
