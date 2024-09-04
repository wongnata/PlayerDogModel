using GameNetcodeStuff;
using HarmonyLib;
using MoreCompany;
using MoreCompany.Cosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerDogModel_Plus.Patches
{
    public static class MoreCompanyPatch
    {
        public static void HideCosmeticsForPlayer(PlayerControllerB playerController)
        {
            CosmeticApplication cosmeticApplication = playerController.meshContainer.GetComponentInChildren<CosmeticApplication>();

            if (cosmeticApplication == null)
            {
                Plugin.logger.LogDebug($"{playerController.playerUsername}'s cosmetic application's instance was null!");
                return;
            }

            Plugin.logger.LogDebug($"{playerController.playerUsername}'s cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");

            cosmeticApplication.ClearCosmetics();

            List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
            foreach (var selected in selectedCosmetics)
            {
                Plugin.logger.LogDebug($"Disabling {playerController.playerUsername}'s {selected}...");
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

            if (cosmeticApplication == null)
            {
                Plugin.logger.LogDebug($"{playerController.playerUsername}'s cosmetic application's instance was null!");
                return;
            }

            Plugin.logger.LogDebug($"{playerController.playerUsername}'s cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");

            cosmeticApplication.ClearCosmetics();
            List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
            foreach (var selected in selectedCosmetics)
            {
                Plugin.logger.LogDebug($"Enabling {playerController.playerUsername}'s {selected}...");
                cosmeticApplication.ApplyCosmetic(selected, true);
            }

            cosmeticApplication.RefreshAllCosmeticPositions();
            foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
            {
                cosmetic.transform.localScale *= CosmeticRegistry.COSMETIC_PLAYER_SCALE_MULT;

            }
        }

        [HarmonyPatch(typeof(CosmeticPatches), nameof(CosmeticPatches.CloneCosmeticsToNonPlayer))]
        class CloneCosmeticsToNonPlayerPatch
        {
            static bool Prefix(Transform cosmeticRoot, int playerClientId)
            {
                Plugin.logger.LogDebug($"Checking for dog mode before copying cosmetics to body...");

                PlayerModelReplacer replacer = null;
                foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
                {
                    var currentReplacer = player.GetComponent<PlayerModelReplacer>();
                    if (currentReplacer != null && (int)currentReplacer.PlayerClientId == playerClientId)
                    {
                        replacer = currentReplacer;
                        break;
                    }
                }

                if (replacer == null)
                {
                    Plugin.logger.LogDebug($"Could not find replacer for playerClientId={playerClientId}. Nothing to prefix.");
                    return true;
                }

                // If this person is a dog, we use this prefix to skip cloning the cosmetics.
                if (replacer.IsDog)
                {
                    Plugin.logger.LogDebug($"playerClientId={playerClientId} is a dog! Skipping cosmetics cloning to body...");
                    return false;
                }

                // Otherwise, we let the cosmetics get cloned.
                Plugin.logger.LogDebug($"playerClientId={playerClientId} is a human! Cloning cosmetics to body...");
                return true;
            }
        }
    }
}
