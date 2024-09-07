using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(JetpackItem))]
    internal class JetpackPatch
    {
        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(JetpackItem __instance, PlayerControllerB ___playerHeldBy, bool ___isHeld)
        {
            if (___playerHeldBy != null && ___isHeld)
            {
                PlayerModelReplacer replacer = ___playerHeldBy.GetComponent<PlayerModelReplacer>();

                if (replacer == null || !replacer.IsDog) return; // Nothing to do.

                Transform dogTorso = replacer.GetDogGameObject().transform.Find("Armature").Find("torso");

                // Need to adjust the arm component of this item to line up with the item anchor
                __instance.transform.position = dogTorso.Find("head").Find("serverItem").position;
                __instance.transform.Rotate(__instance.backPartRotationOffset);

                __instance.backPart.rotation = dogTorso.rotation;
                __instance.backPart.position = dogTorso.position;

                Vector3 vector = __instance.backPartPositionOffset;
                vector = dogTorso.rotation * vector;
                __instance.backPart.position += vector;
            }
        }
    }
}
