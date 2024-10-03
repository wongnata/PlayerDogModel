using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Config;
using PlayerDogModel_Plus.Source.Networking;
using PlayerDogModel_Plus.Source.Patches.Core;
using PlayerDogModel_Plus.Source.Patches.Optional;
using PlayerDogModel_Plus.Source.Terminal;
using System.IO;
using System.Reflection;
using UnityEngine;
using static BepInEx.BepInDependency;

namespace PlayerDogModel_Plus.Source
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("LethalNetworkAPI")]
    [BepInDependency("me.swipez.melonloader.morecompany", DependencyFlags.SoftDependency)]
    [BepInDependency("verity.3rdperson", DependencyFlags.SoftDependency)]
    [BepInDependency("Zaggy1024.OpenBodyCams", DependencyFlags.SoftDependency)]
    [BepInDependency("FlipMods.TooManyEmotes", DependencyFlags.SoftDependency)]
    [BepInDependency("atomic.terminalapi", DependencyFlags.SoftDependency)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Harmony harmony;
        internal static PluginConfig config { get; private set; } = null!;
        internal static ManualLogSource logger;
        internal static AssetBundle assetBundle { get; private set; }
        internal static bool isMoreCompanyLoaded = false;
        internal static bool isThirdPersonLoaded = false;
        internal static bool isMirageLoaded = false;
        internal static bool isOpenBodyCamsLoaded = false;
        internal static bool isTooManyEmotesLoaded = false;
        internal static bool isTerminalApiLoaded = false;

        private void Awake()
        {
            logger = Logger;
            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(JetpackPatch));
            harmony.PatchAll(typeof(SnareFleaPatch));
            harmony.PatchAll(typeof(SpectateCameraPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(UnlockableSuitPatch));
            harmony.PatchAll(typeof(DeadBodyPatch));
            harmony.PatchAll(typeof(BeltBagPatch));
            harmony.PatchAll(typeof(HauntedMaskPatch));
            harmony.PatchAll(typeof(MaskedPlayerEnemyPatch));
            harmony.PatchAll(typeof(FlowerSnakeEnemyPatch));
            logger.LogInfo($"loaded core patches...");

            if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
            {
                isMoreCompanyLoaded = true;
                harmony.PatchAll(typeof(MoreCompanyPatch));
                logger.LogInfo($"loaded MoreCompany patches...");

                // This looks a bit psychotic but we only patch this to add more company cosmetic support
                if (Chainloader.PluginInfos.ContainsKey("FlipMods.TooManyEmotes"))
                {
                    isTooManyEmotesLoaded = true;
                    harmony.PatchAll(typeof(TooManyEmotesPatch));
                    logger.LogInfo($"loaded TooManyEmotes patches...");
                }
            }

            if (Chainloader.PluginInfos.ContainsKey("verity.3rdperson"))
            {
                isThirdPersonLoaded = true;
                harmony.PatchAll(typeof(ThirdPersonPatch));
                logger.LogInfo($"loaded 3rdPerson patches...");
            }

            if (Chainloader.PluginInfos.ContainsKey("Zaggy1024.OpenBodyCams"))
            {
                isOpenBodyCamsLoaded = true;
                harmony.PatchAll(typeof(OpenBodyCamsPatch));
                logger.LogInfo($"loaded OpenBodyCamsPatch patches...");
            }

            if (Chainloader.PluginInfos.ContainsKey("Mirage"))
            {
                logger.LogDebug($"detected Mirage...");
                isMirageLoaded = true;
            }

            if (Chainloader.PluginInfos.ContainsKey("atomic.terminalapi"))
            {
                logger.LogDebug($"detected TerminalAPI...");
                SwitchModelCommand.Initialize();
                isTerminalApiLoaded = true;
            }

            logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded successfully! Woof!");

            config = new PluginConfig(Config);

            MessageHandler.Initialize();
            assetBundle = AssetBundle.LoadFromFile(GetAssemblyFullPath("playerdog"));
        }

        private static string GetAssemblyFullPath(string additionalPath)
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = additionalPath != null ? Path.Combine(directoryName, ".\\" + additionalPath) : directoryName;
            return Path.GetFullPath(path);
        }
    }
}