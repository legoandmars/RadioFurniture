using HarmonyLib;
using RadioFurniture.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RadioFurniture.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    public class RoundManagerPatches
    {
        [HarmonyPatch("SetToCurrentLevelWeather")]
        [HarmonyPostfix]
        static void SetToCurrentLevelWeather(ref SelectableLevel ___currentLevel)
        {
            if (___currentLevel.currentWeather == LevelWeatherType.Stormy)
            {
                WeatherEvents.StormStart();
            }
        }
    }
}
