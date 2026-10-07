using System;

namespace PlowParty.Gameplay.Snow.Network
{
    public interface ISnowClock
    {
        bool IsPlaying { get; }

        float PlayingElapsed { get; }

        event Action MatchRestarted;
    }
}
