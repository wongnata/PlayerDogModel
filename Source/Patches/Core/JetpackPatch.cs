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
                __instance.transform.position = dogTorso.Find("head").Find("serverItem").position;
                __instance.backPart.position = dogTorso.position + dogTorso.up * 1.0f + dogTorso.right * -0.03f;
            }
        }
    }
}
