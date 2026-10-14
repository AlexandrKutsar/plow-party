using System.Collections.Generic;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Party
{
    public sealed class PartyMemory
    {
        private readonly List<PartyBot> _bots = new List<PartyBot>();
        private readonly List<string> _joinOrder = new List<string>();

        public string Code { get; private set; }

        public PartyRole Role { get; private set; }

        public PartyMode Mode { get; private set; }

        public bool IsReady { get; private set; }

        public bool HasParty => Code != null;

        public PartyMode SoloMode { get; private set; }

        public bool HasLook { get; private set; }

        public int Color { get; private set; }

        public int Critter { get; private set; }

        public IReadOnlyList<PartyBot> Bots => _bots;

        public IReadOnlyList<string> JoinOrder => _joinOrder;

        public void Remember(string code, PartyRole role, PartyMode mode, bool isReady)
        {
            Code = code;
            Role = role;
            Mode = mode;
            IsReady = isReady;
        }

        public void RememberJoinOrder(IEnumerable<string> keys)
        {
            _joinOrder.Clear();
            _joinOrder.AddRange(keys);
        }

        public void RememberSetup(int color, int critter, IEnumerable<PartyBot> bots)
        {
            HasLook = true;
            Color = color;
            Critter = critter;
            _bots.Clear();
            _bots.AddRange(bots);
        }

        public void RememberSoloMode(PartyMode mode)
        {
            SoloMode = mode;
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
            _joinOrder.Clear();
        }
    }
}
