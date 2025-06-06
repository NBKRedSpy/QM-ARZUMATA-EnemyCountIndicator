using HarmonyLib;
using MGSC;
using ModConfigMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QM_EnemyCountIndicator
{
    public static class Plugin
    {

        public static ConfigDirectories ConfigDirectories = new ConfigDirectories();

        public static ModConfig Config { get; private set; }

        public static Logger Logger = new Logger();

        // MCM Related Start
        public static string ConfigPath = Path.Combine(ConfigDirectories.ModPersistenceFolder, "config.ini");
        // MCM Related End
        
        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfig(IModContext context)
        {
            Directory.CreateDirectory(ConfigDirectories.ModPersistenceFolder);
            Config = ModConfig.LoadConfig(ConfigDirectories.ConfigPath);
       
            // MCM Related Start

            Logger.Log($"ConfigPath: {ConfigPath}");

            if (!File.Exists(Plugin.ConfigPath))
            {
                Logger.LogWarning($"ConfigPath DOES NOT EXIST!: {ConfigPath}");

                // Assuming the resource name is "config.ini" and it's under Resources folder
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QM_EnemyCountIndicator.config.ini"))
                {
                    if (stream != null)
                    {
                        StreamReader reader = new StreamReader(stream);
                        File.WriteAllText(Plugin.ConfigPath, reader.ReadToEnd());
                    }
                }
            }

            bool flag = false;
            string text = string.Empty;

            try
            {
                flag = Plugin.RegisterToMCM();
                text = Plugin.GetMCMPath();
            }
            catch (Exception)
            {
                Debug.LogWarning("Loading without MCM.");
                flag = false;
                text = string.Empty;
            }

            Plugin.Logger.Log($"\t flag {flag}");

            Plugin.Logger.Log($"\t text {text}");
            Plugin.Logger.Log($"\t File.Exists(text) {File.Exists(text)}");

            if (flag && File.Exists(text))
            {
                Plugin.Config.LoadConfigMCM(text);
                return;
            }

            Plugin.Config.LoadConfigMCM(Plugin.ConfigPath);

            // MCM Related End

        }

        [Hook(ModHookType.AfterBootstrap)]
        public static void Bootstrap(IModContext context)
        {
            new Harmony("ARZUMATA_" + ConfigDirectories.ModAssemblyName).PatchAll();
        }

        // MCM Related Start
        private static bool RegisterToMCM()
        {
            ModConfigMenuAPI.RegisterModConfig("Enemy Indicator", Plugin.ConfigPath, delegate (Dictionary<string, object> properties)
            {
                Plugin.Config.LoadConfigMCM(properties);
            });
            return true;
        }

        private static string GetMCMPath()
        {
            return ModConfigMenuAPI.GetNameForConfigFile(Plugin.ConfigPath);
        }

        // MCM Related End
    }
}
