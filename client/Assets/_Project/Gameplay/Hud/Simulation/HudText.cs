using System.Globalization;
using UnityEngine;

namespace PlowParty.Gameplay.Hud.Simulation
{
    public static class HudText
    {
        public const string Go = "СТАРТ!";

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

        public static string WaitingForPlayers(int seated, int slots)
        {
            return $"Матч скоро начнётся. Ожидание игроков {seated}/{slots}";
        }

        public static string BlizzardIn(int seconds)
        {
            return $"Метель через {seconds}!";
        }

        public static string ScoreGain(int gained)
        {
            return "+" + gained;
        }

        public static string Place(int place)
        {
            return place + "-е";
        }
    }
}
