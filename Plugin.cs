using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using PlayerDogModel_Plus.Config;
using PlayerDogModel_Plus.Patches.Core;
using PlayerDogModel_Plus.Patches.Optional;
using System.IO;
using System.Reflection;
using static BepInEx.BepInDependency;

namespace PlayerDogModel_Plus
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("LC_API_V50")]
    [BepInDependency("x753.More_Suits")]
    [BepInDependency("me.swipez.melonloader.morecompany", DependencyFlags.SoftDependency)]
    [BepInDependency("verity.3rdperson", DependencyFlags.SoftDependency)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Harmony harmony;
        internal static PlayerDogModelConfig boundConfig { get; private set; } = null!;
        internal static ManualLogSource logger;

        private void Awake()
        {
            logger = base.Logger;
            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(JetpackPatch.LateUpdatePatch));
            harmony.PatchAll(typeof(SnareFleaPatch.UpdatePositionToClingingPlayerHeadPatch));
            harmony.PatchAll(typeof(SpectateCameraPatch.RaycastSpectateCameraAroundPivotPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(UnlockableSuitPatch));
            harmony.PatchAll(typeof(DeadBodyPatch));
            logger.LogInfo($"loaded core patches...");

            if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
            {
                harmony.PatchAll(typeof(MoreCompanyPatch.CloneCosmeticsToNonPlayerPatch));
                logger.LogInfo($"loaded MoreCompany patches...");
            }

            if (Chainloader.PluginInfos.ContainsKey("verity.3rdperson"))
            {
                harmony.PatchAll(typeof(ThirdPersonPatch.ThirdPersonUpdatePatch));
                harmony.PatchAll(typeof(ThirdPersonPatch.ThirdPersonOrbitUpdatePatch));
                logger.LogInfo($"loaded 3rdPerson patches...");
            }

            logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded! Woof!");

            boundConfig = new PlayerDogModelConfig(base.Config);

            Networking.Initialize();
            LC_API.BundleAPI.BundleLoader.LoadAssetBundle(GetAssemblyFullPath("playerdog"));
        }

        private static string GetAssemblyFullPath(string additionalPath)
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = ((additionalPath != null) ? Path.Combine(directoryName, ".\\" + additionalPath) : directoryName);
            return Path.GetFullPath(path);
        }
    }
}