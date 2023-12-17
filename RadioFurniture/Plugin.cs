using BepInEx;
using RadioBrowser.Models;
using RadioBrowser;
using System.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine;
using RadioFurniture.Managers;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace RadioFurniture
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        private void Awake()
        {
            Instance = this;
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            var radioManagerObject = new GameObject("RadioManager");
            UnityEngine.Object.DontDestroyOnLoad(radioManagerObject);
            radioManagerObject.hideFlags = HideFlags.HideAndDontSave;
            radioManagerObject.AddComponent<RadioManager>();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static void Log(string log)
        {
            Instance.Logger.LogInfo(log);
        }
    }
}