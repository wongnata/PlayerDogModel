using GameNetcodeStuff;
using HarmonyLib;
using OpenBodyCams;
using OpenBodyCams.Compatibility;
using PlayerDogModel_Plus.Source.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Optional
{
    internal class OpenBodyCamsPatch
    {
        private static readonly Vector3 cameraContainerOffset = new(0, 0, 0.125f);

        [HarmonyPatch(typeof(BodyCamComponent), "SetTargetToPlayer")]
        [HarmonyPostfix]
        public static void SetTargetToPlayerPostfix(PlayerControllerB player, GameObject ___CameraObject, ref Renderer[] ___currentRenderersToHide)
        {
            if (player.isPlayerDead) return;

            PlayerModelReplacer replacer = player.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return; // Nothing to patch

            ___CameraObject.transform.localPosition = cameraContainerOffset;

            // Hide the dog model from the camera
            ___currentRenderersToHide = ___currentRenderersToHide.Concat(replacer.dogRenderers).ToArray();
        }

        [HarmonyPatch(typeof(MoreCompanyCompatibility), "CollectCosmetics")]
        [HarmonyPrefix]
        public static bool CollectCosmeticsPrefix(PlayerControllerB player, ref IEnumerable<GameObject> __result)
        {
            PlayerModelReplacer replacer = player.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return true; // Nothing to patch

            __result = Enumerable.Empty<GameObject>();
            return false; // Collect no cosmetics for dogs
        }
    }
}
