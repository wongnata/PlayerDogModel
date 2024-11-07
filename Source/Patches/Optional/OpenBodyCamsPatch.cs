using GameNetcodeStuff;
using HarmonyLib;
using OpenBodyCams;
using OpenBodyCams.API;
using PlayerDogModel_Plus.Source.Model;
using System.Linq;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Optional
{
    internal class OpenBodyCamsPatch
    {
        private static readonly Vector3 cameraContainerOffset = new(0, 0, 0.125f);

        public static void Initialize()
        {
            BodyCam.PlayerThirdPersonCosmeticsGetters += GetDogRenderers;
        }

        [HarmonyPatch(typeof(BodyCamComponent), "SetTargetToPlayer")]
        [HarmonyPostfix]
        public static void SetTargetToPlayerPostfix(PlayerControllerB player, Transform ___CameraTransform, ref Renderer[] ___currentRenderersToHide)
        {
            if (player.isPlayerDead) return;

            PlayerModelReplacer replacer = player.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return; // Nothing to patch

            ___CameraTransform.localPosition = cameraContainerOffset;
        }

        internal static GameObject[] GetDogRenderers(PlayerControllerB player)
        {
#if DEBUG
            Plugin.logger.LogDebug($"Callback invoked for OpenBodyCam camera that's targetting {player.playerUsername}!");
#endif
            if (player.isPlayerDead) return [];

            PlayerModelReplacer replacer = player.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return [];

#if DEBUG
            Plugin.logger.LogDebug($"{player.playerUsername} is a dog, returning their dog model to be hidden as a third party cosmetic!");
#endif
            SkinnedMeshRenderer[] dogRenderers = replacer.dogRenderers;
            return dogRenderers.Select(renderer => renderer.gameObject).ToArray();
        }
    }
}
