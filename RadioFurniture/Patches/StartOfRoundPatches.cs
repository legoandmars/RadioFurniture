using HarmonyLib;
using RadioFurniture.Behaviour;
using RadioFurniture.Events;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

        [HarmonyPatch(nameof(StartOfRound.PowerSurgeShip))]
        [HarmonyPostfix]
        static void PowerSurgeShip()
        {
            try
            {
                var radioBehaviour = UnityEngine.Object.FindObjectOfType<RadioBehaviour>();
                if (radioBehaviour != null)
                {
                    radioBehaviour.TurnOffRadioServerRpc();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Something went wrong forcing radio off...");
                Debug.LogWarning(ex);
            }
        }

        [HarmonyPatch(nameof(StartOfRound.SyncShipUnlockablesClientRpc))]
        [HarmonyPostfix]
        static void SyncShipUnlockablesClientRpc()
        {
            try
            {
                var radioBehaviour = UnityEngine.Object.FindObjectOfType<RadioBehaviour>();
                if (radioBehaviour != null)
                {
                    radioBehaviour.SyncRadioServerRpc();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Something went wrong forcing radio sync...");
                Debug.LogWarning(ex);
            }
        }
    }
}
