using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(CentipedeAI))]
    internal class SnareFleaPatch
    {
        [HarmonyPatch("UpdatePositionToClingingPlayerHead")]
        [HarmonyPostfix]
        public static void UpdatePositionToClingingPlayerHeadPostfix(CentipedeAI __instance, bool ___clingingToLocalClient, ref PlayerControllerB ___clingingToPlayer)
        {
            if (___clingingToLocalClient) return; // Local camera is fine here.

            PlayerModelReplacer replacer = ___clingingToPlayer.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return; // Nothing to do.

            if (replacer.GetDogGameObject() == null)
            {
#if DEBUG
                Plugin.logger.LogDebug($"dog game object was null when trying to apply centipede.");
#endif
                return;
            }

            Transform dogHead = replacer.GetDogGameObject().transform.Find("Armature").Find("torso").Find("head");
            __instance.transform.position = dogHead.position + dogHead.up * 0.38f;
            __instance.transform.eulerAngles = dogHead.eulerAngles;
        }
    }
}
