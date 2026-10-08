using PlowParty.Meta.Party.Simulation;

namespace PlowParty.Meta.Party
{
    public sealed class PartyMemory
    {
        public string Code { get; private set; }

        public PartyRole Role { get; private set; }

        public PartyMode Mode { get; private set; }

        public bool HasParty => Code != null;

        public void Remember(string code, PartyRole role, PartyMode mode)
        {
            Code = code;
            Role = role;
            Mode = mode;
        }

        public void Forget()
        {
            Code = null;
            Role = PartyRole.Member;
            Mode = PartyMode.QuickPlay;
        }
    }
}
