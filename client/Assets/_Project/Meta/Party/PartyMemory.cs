using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Party
{
    public sealed class PartyMemory
    {
        public string Code { get; private set; }

        public PartyRole Role { get; private set; }

        public PartyMode Mode { get; private set; }

        public bool IsReady { get; private set; }

        public bool HasParty => Code != null;

        public void Remember(string code, PartyRole role, PartyMode mode, bool isReady)
        {
            Code = code;
            Role = role;
            Mode = mode;
            IsReady = isReady;
        }

        public void ForgetReady()
        {
            IsReady = false;
        }

        public void Forget()
        {
            Code = null;
            Role = PartyRole.Member;
            Mode = PartyMode.QuickPlay;
            IsReady = false;
        }
    }
}
