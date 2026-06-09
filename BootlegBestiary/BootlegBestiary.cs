using BepInEx;
using BepInEx.Bootstrap;
using System.Collections.Generic;
using BootlegBestiary.Shared.Assets;
using System;
using BepInEx.Configuration;
using RoR2;

//still cleanup to do...

namespace BootlegBestiary
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]


    public class BootlegBestiary : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Skeletogne";
        public const string PluginName = "BootlegBestiary";
        public const string PluginVersion = "1.0.0";
        internal static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
        public static BootlegBestiary Instance { get; private set; }
        internal string DirectoryName => System.IO.Path.GetDirectoryName(Info.Location);
        public Dictionary<Type, ConfigEntry<bool>> mainModuleConfigEntries = new Dictionary<Type, ConfigEntry<bool>>();
        public class ModuleInfoAttribute : Attribute
        {
            public string ModuleName { get; }
            public string Description { get; }
            public string Section { get; }
            public bool RequiresRestart { get; }
            public ModuleInfoAttribute(string moduleName, string description, string section, bool requiresRestart)
            {
                ModuleName = moduleName;
                Description = description;
                Section = section;
                RequiresRestart = requiresRestart;
            }
        }
        public void Awake()
        {
            Instance = this;
            Log.Init(Logger);
            PluginConfig.Init(Config);
            VanillaAssets.Init();
            CustomAssets.Init();
            ClonedAssets.Init();
            foreach (var kvp in mainModuleConfigEntries)
            {
                Type type = kvp.Key;
                Instance.gameObject.AddComponent(type);
            }
        }
    }
}

