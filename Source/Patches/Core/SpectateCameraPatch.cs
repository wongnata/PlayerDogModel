using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Util;
using System;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class SpectateCameraPatch
    {
        [HarmonyPatch("RaycastSpectateCameraAroundPivot")]
        [HarmonyPrefix]
        public static void RaycastSpectateCameraAroundPivotPrefix(ref Transform ___spectateCameraPivot, PlayerControllerB ___spectatedPlayerScript)
        {
            try
            {
                PlayerModelReplacer replacer = ModelReplacerRetriever.GetModelReplacerFromClientId(___spectatedPlayerScript.playerClientId);

                if (replacer == null || !replacer.IsDog) return; // Nothing to do.

                ___spectateCameraPivot.position = replacer.GetDogTorso().position + Vector3.up * 0.5f;
            }
            catch (Exception e)
            {
                if (!Plugin.boundConfig.suppressExceptions.Value) throw e; // Couldn't adjust the spectator camera, no biggie.
            }
        }

        [HarmonyPatch("RaycastSpectateCameraAroundPivot")]
        [HarmonyPostfix]
        public static void RaycastSpectateCameraAroundPivotPostfix(ref Transform ___spectateCameraPivot, PlayerControllerB ___spectatedPlayerScript)
        {
            try
            {
                PlayerModelReplacer replacer = null;
                foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
                {
                    var currentReplacer = player.GetComponent<PlayerModelReplacer>();
                    if (currentReplacer != null && currentReplacer.PlayerClientId == ___spectatedPlayerScript.playerClientId)
                    {
                        replacer = currentReplacer;
                        break;
                    }
                }

                if (replacer == null || !replacer.IsDog) return; // Nothing to do.

                ___spectateCameraPivot.GetComponentInChildren<Camera>().transform.localPosition = Vector3.back * 1.8f;
            }
            catch (Exception e)
            {
                if (!Plugin.boundConfig.suppressExceptions.Value) throw e; // Couldn't adjust the spectator camera, no biggie.
            }
        }
    }
}
