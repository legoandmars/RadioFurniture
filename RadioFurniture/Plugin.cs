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
using System.IO;
using LethalLib.Extras;

namespace RadioFurniture
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("evaisa.lethallib")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            var assets = LoadAssets();
            if(assets == null)
            {
                Logger.LogInfo("Failed to load radio because assetbundle does not exist. Disabling!");
                return;
            }

            RegisterRPCs();
            LethalLib.Modules.Unlockables.RegisterUnlockable(assets.unlockable, 1, LethalLib.Modules.StoreType.Decor);
            RadioManager.PreloadStations();

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            return;
        }

        private void RegisterRPCs()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        private UnlockableItemDef? LoadAssets()
        {
            var bundlePath = Path.Join(Path.GetDirectoryName(this.Info.Location), "radio");
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            var unlockableItemDef = bundle.LoadAsset<UnlockableItemDef>("assets/data/radio.asset");
            return unlockableItemDef;
        }

        public static void Log(string log)
        {
            Instance.Logger.LogInfo(log);
        }
    }
}