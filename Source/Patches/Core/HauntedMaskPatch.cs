using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(HauntedMaskItem))]
    internal class HauntedMaskPatch
    {
        [HarmonyPatch("PositionHeadMaskWithOffset")]
        [HarmonyPostfix]
        public static void PositionHeadMaskWithOffsetPostfix(HauntedMaskItem __instance,
            Transform ___currentHeadMask,
            PlayerControllerB ___previousPlayerHeldBy,
            Vector3 ___headRotationOffset,
            Vector3 ___headPositionOffset)
        {
            if (__instance.IsOwner) return; // Nothing to do, animation already targets player camera.

            PlayerModelReplacer replacer = ___previousPlayerHeldBy.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return;

            Transform dogHead = replacer.GetDogGameObject().transform.Find("Armature").Find("torso").Find("head");

            ___currentHeadMask.rotation = dogHead.rotation;
            ___currentHeadMask.position = dogHead.position + dogHead.forward * 0.5f + dogHead.up * 0.2f;
            ___currentHeadMask.Rotate(-35, 0, 0);
        }

        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(ref HauntedMaskItem __instance, PlayerControllerB ___playerHeldBy, bool ___isHeld)
        {
            if (___playerHeldBy != null && ___isHeld)
            {
                PlayerModelReplacer replacer = ___playerHeldBy.GetComponent<PlayerModelReplacer>();

                if (replacer == null || !replacer.IsDog) return; // Nothing to do.

                __instance.transform.position = replacer.itemAnchor.position + replacer.itemAnchor.forward * 0.1f;

            }
        }
    }
}
