using System.Collections.Generic;

namespace PlowParty.Meta.Tournament.Network
{
    public interface IMatchProgress
    {
        bool IsRunning { get; }

        MatchProgressPhase Phase { get; }

        int MatchNumber { get; }

        float PlayingElapsed { get; }

        void ReadFinalScores(Dictionary<int, int> scoresBySlot);

        void ReadLiveScores(Dictionary<int, int> scoresBySlot);
    }
}
