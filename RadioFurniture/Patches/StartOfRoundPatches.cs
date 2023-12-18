using HarmonyLib;
using RadioFurniture.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RadioFurniture.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatches
    {
        [HarmonyPatch("EndOfGame")]
        [HarmonyPrefix]
        static void EndOfGame(ref SelectableLevel ___currentLevel)
        {
            if (___currentLevel.currentWeather == LevelWeatherType.Stormy)
            {
                WeatherEvents.StormEnd();
            }
        }
    }
}
