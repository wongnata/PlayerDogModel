using GameNetcodeStuff;
using HarmonyLib;
using OpenBodyCams;
using PlayerDogModel_Plus.Source.Model;
using System.Linq;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Optional
{
    [HarmonyPatch(typeof(BodyCamComponent))]
    internal class OpenBodyCamsPatch
    {
        private static readonly Vector3 cameraContainerOffset = new(0, 0, 0.125f);

        internal static void Update(Transform dogObject)
        {
            BodyCamComponent.MarkTargetDirtyUntilRenderForAllBodyCams(dogObject);
        }

        [HarmonyPatch("SetTargetToPlayer")]
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
    }
}
