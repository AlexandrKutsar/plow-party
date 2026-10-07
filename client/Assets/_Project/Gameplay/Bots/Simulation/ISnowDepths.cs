using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public interface ISnowDepths
    {
        Vector2 Origin { get; }

        float CellSize { get; }

        int Width { get; }

        int Height { get; }

        int FullDepth { get; }

        int GetDepth(int x, int y);
    }
}
