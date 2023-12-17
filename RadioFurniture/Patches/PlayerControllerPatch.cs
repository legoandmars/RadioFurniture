using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RadioFurniture.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPatch(PlayerControllerB __instance)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "cube";
            cube.transform.position = __instance.transform.position;
            cube.GetComponent<Renderer>().material.shader = __instance.GetComponentInChildren<Renderer>().material.shader;
        }
    }
}
