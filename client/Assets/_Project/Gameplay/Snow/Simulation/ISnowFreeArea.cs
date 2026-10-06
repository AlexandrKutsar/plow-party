using UnityEngine;

namespace PlowParty.Gameplay.Snow.Simulation
{
    public interface ISnowFreeArea
    {
        bool Contains(Vector2 point);
    }
}
