using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace PlayerDogModel_Plus.Patches.Core
{
    internal class SnareFleaPatch
    {
        [HarmonyPatch(typeof(CentipedeAI))]
        [HarmonyPatch("UpdatePositionToClingingPlayerHead")]
        internal class UpdatePositionToClingingPlayerHeadPatch
        {
            static void Postfix(CentipedeAI __instance, bool ___clingingToLocalClient, ref PlayerControllerB ___clingingToPlayer)
            {
                if (___clingingToLocalClient) return; // Local camera is fine here.

                PlayerModelReplacer replacer = null;
                foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
                {
                    var currentReplacer = player.GetComponent<PlayerModelReplacer>();
                    if (currentReplacer != null && currentReplacer.PlayerClientId == ___clingingToPlayer.playerClientId)
                    {
                        replacer = currentReplacer;
                        break;
                    }
                }

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
}
