using System.Collections.Generic;
using PlowParty.Meta.Tournament.Simulation;

namespace PlowParty.Meta.Tournament.Network
{
    public interface IMatchRoster
    {
        IReadOnlyList<RosterSeat> ReadSeats();
    }
}
