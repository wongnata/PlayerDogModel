using GameNetcodeStuff;
using HarmonyLib;
using MoreCompany;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlayerDogModel_Plus.Patches
{
    internal class SpectateCameraPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB))]
        [HarmonyPatch("RaycastSpectateCameraAroundPivot")]
        class RaycastSpectateCameraAroundPivotPatch
        {
            static void Prefix(ref Transform ___spectateCameraPivot, PlayerControllerB ___spectatedPlayerScript)
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

                    ___spectateCameraPivot.position = replacer.GetDogTorso().position + Vector3.up * 0.5f;
                }
                catch
                {
                    // Couldn't adjust the spectator camera, no biggie.
                }
            }

            static void Postfix(ref Transform ___spectateCameraPivot, PlayerControllerB ___spectatedPlayerScript)
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
                catch
                {
                    // Couldn't adjust the spectator camera, no biggie.
                }
            }
        }
    }
}
