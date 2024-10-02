using GameNetcodeStuff;
using HarmonyLib;
using MoreCompany.Cosmetics;
using PlayerDogModel_Plus.Source.Model;
using TooManyEmotes.Patches;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Optional
{
    [HarmonyPatch(typeof(ThirdPersonEmoteController))]
    internal class TooManyEmotesPatch
    {
        [HarmonyPatch("OnStartCustomEmoteLocal")]
        [HarmonyPostfix]
        internal static void OnStartCustomEmoteLocalPostfix()
        {
            if (!Plugin.isMoreCompanyLoaded) return;

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            PlayerModelReplacer replacer = localPlayer.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return;

            HideLocalCosmetics();
        }

        [HarmonyPatch("UpdateFirstPersonEmoteMode")]
        [HarmonyPostfix]
        internal static void UpdateFirstPersonEmoteModePostfix()
        {
            if (!Plugin.isMoreCompanyLoaded) return;

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            PlayerModelReplacer replacer = localPlayer.GetComponent<PlayerModelReplacer>();


            if (replacer == null || !replacer.IsDog) return;

            HideLocalCosmetics();
        }

        // Ripped directly from TME more or less
        private static void HideLocalCosmetics()
        {
            Transform cosmeticRoot = GameNetworkManager.Instance.localPlayerController.transform;
            var cosmeticApplication = cosmeticRoot?.GetComponentInChildren<CosmeticApplication>();

            if (cosmeticApplication && cosmeticApplication.spawnedCosmetics.Count != 0)
            {
                foreach (var item in cosmeticApplication.spawnedCosmetics)
                    SetAllChildrenLayer(item.transform, 23);
            }
        }

        // Ripped directly from TME
        private static void SetAllChildrenLayer(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            foreach (var light in transform.gameObject.GetComponents<Light>())
                light.cullingMask = 1 << layer;

            foreach (Transform item in transform)
                SetAllChildrenLayer(item, layer);
        }
    }
}
