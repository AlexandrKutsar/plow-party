using System.Collections.Generic;

namespace PlowParty.Meta.Tournament.Simulation
{
    public sealed class TournamentBoard
    {
        public TournamentBoard(IReadOnlyList<Standing> rows, int myRank, int players, Medal medal)
        {
            Rows = rows;
            MyRank = myRank;
            Players = players;
            Medal = medal;
        }

        public IReadOnlyList<Standing> Rows { get; }

        public int MyRank { get; }

        public int Players { get; }

        public Medal Medal { get; }
    }
}
