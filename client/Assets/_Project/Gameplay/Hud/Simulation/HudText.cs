using System.Globalization;
using UnityEngine;

namespace PlowParty.Gameplay.Hud.Simulation
{
    public static class HudText
    {
        public static int WholeSecondsLeft(float seconds)
        {
            return Mathf.Max(0, Mathf.CeilToInt(seconds - 1e-4f));
        }

        public static string Clock(int wholeSeconds)
        {
            return $"{wholeSeconds / 60}:{wholeSeconds % 60:00}";
        }

        public static string Multiplier(float multiplier)
        {
            return "×" + multiplier.ToString("0.#", CultureInfo.InvariantCulture);
        }

        public static string ParticipantName(int slot, bool isLocal)
        {
            return isLocal ? "You" : $"Player {slot + 1}";
        }

        public static string Place(int place)
        {
            switch (place % 100)
            {
                case 11:
                case 12:
                case 13:
                    return place + "th";
            }

            switch (place % 10)
            {
                case 1:
                    return place + "st";
                case 2:
                    return place + "nd";
                case 3:
                    return place + "rd";
                default:
                    return place + "th";
            }
        }
    }
}
