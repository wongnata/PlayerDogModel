using GameNetcodeStuff;
using HarmonyLib;
using MoreCompany;
using MoreCompany.Cosmetics;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Util;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Optional
{
    [HarmonyPatch(typeof(CosmeticPatches))]
    internal class MoreCompanyPatch
    {
        [HarmonyPatch("CloneCosmeticsToNonPlayer")]
        [HarmonyPrefix]
        public static bool CloneCosmeticsToNonPlayerPrefix(Transform cosmeticRoot, int playerClientId)
        {
            PlayerModelReplacer replacer = ModelReplacerRetriever.GetModelReplacerFromClientId((ulong)playerClientId);

            if (replacer == null || !replacer.IsDog)
            {
                return true; // Nothing to prefix
            }

            return false;
        }

        public static void HideCosmeticsForPlayer(PlayerControllerB playerController)
        {
            CosmeticApplication cosmeticApplication = playerController.meshContainer.GetComponentInChildren<CosmeticApplication>();

            if (cosmeticApplication == null) return;

#if DEBUG
            Plugin.logger.LogDebug($"{playerController.playerUsername}'s cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");
#endif

            cosmeticApplication.ClearCosmetics();

            List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
            foreach (var selected in selectedCosmetics)
            {
#if DEBUG
                Plugin.logger.LogDebug($"Disabling {playerController.playerUsername}'s {selected}...");
#endif
                cosmeticApplication.ApplyCosmetic(selected, false);
            }

            cosmeticApplication.RefreshAllCosmeticPositions();
            foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
            {
                cosmetic.transform.localScale *= CosmeticRegistry.COSMETIC_PLAYER_SCALE_MULT;
            }
        }

        public static void ShowCosmeticsForPlayer(PlayerControllerB playerController)
        {
            CosmeticApplication cosmeticApplication = playerController.meshContainer.GetComponentInChildren<CosmeticApplication>();

            if (cosmeticApplication == null) return;

#if DEBUG
            Plugin.logger.LogDebug($"{playerController.playerUsername}'s cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");
#endif

            cosmeticApplication.ClearCosmetics();
            List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
            foreach (var selected in selectedCosmetics)
            {
#if DEBUG
                Plugin.logger.LogDebug($"Enabling {playerController.playerUsername}'s {selected}...");
#endif
                cosmeticApplication.ApplyCosmetic(selected, true);
            }

            cosmeticApplication.RefreshAllCosmeticPositions();
            foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
            {
                cosmetic.transform.localScale *= CosmeticRegistry.COSMETIC_PLAYER_SCALE_MULT;

            }
        }
    }
}
