using PlowParty.Infrastructure.Network;

namespace PlowParty.Meta.Party.Simulation
{
    public static class PartyReturnRules
    {
        public static PartyReturnStep First(PartyRole role)
        {
            return role == PartyRole.Leader ? PartyReturnStep.Host : PartyReturnStep.Join;
        }

        public static PartyReturnStep Next(PartyRole role, PartyReturnStep last, SessionStartOutcome outcome, float elapsedSeconds, float rejoinSeconds, float giveUpSeconds)
        {
            if (outcome == SessionStartOutcome.Full || elapsedSeconds >= giveUpSeconds)
            {
                return PartyReturnStep.Stop;
            }

            if (RestartsWait(outcome))
            {
                return PartyReturnStep.Join;
            }

            if (last == PartyReturnStep.Host)
            {
                return outcome == SessionStartOutcome.NameTaken ? PartyReturnStep.Join : PartyReturnStep.Host;
            }

            return role == PartyRole.Leader || elapsedSeconds >= rejoinSeconds ? PartyReturnStep.Host : PartyReturnStep.Join;
        }

        public static bool RestartsWait(SessionStartOutcome outcome)
        {
            return outcome == SessionStartOutcome.Refused;
        }

        public static PartyRole RoleAfterLeaderLeft(PartyState lastKnown, int localId)
        {
            var party = PartyState.Restore(lastKnown.Count, lastKnown.Mode, lastKnown.Members);
            party.Leave(party.LeaderId);
            return party.IsLeader(localId) ? PartyRole.Leader : PartyRole.Member;
        }
    }
}
