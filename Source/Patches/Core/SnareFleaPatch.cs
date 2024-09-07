using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Util;
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

            PlayerModelReplacer replacer = ModelReplacerRetriever.GetModelReplacerFromClientId(___clingingToPlayer.playerClientId);

            if (replacer == null || !replacer.IsDog) return; // Nothing to do.

            if (replacer.GetDogGameObject() == null)
            {
                Plugin.logger.LogDebug($"dog game object was null when trying to apply centipede.");
                return;
            }

            Transform dogHead = replacer.GetDogGameObject().transform.Find("Armature").Find("torso").Find("head");
            __instance.transform.position = dogHead.position + dogHead.up * 0.38f;
            __instance.transform.eulerAngles = dogHead.eulerAngles;
        }
    }
}
