using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(BeltBagItem))]
    internal class BeltBagPatch
    {
        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(ref BeltBagItem __instance, PlayerControllerB ___playerHeldBy, bool ___isHeld, bool ___isPocketed)
        {
            if (___playerHeldBy != null && ___isHeld)
            {
                PlayerModelReplacer replacer = ___playerHeldBy.GetComponent<PlayerModelReplacer>();

                if (replacer == null || !replacer.IsDog) return; // Nothing to do.

                if (!___isPocketed)
                {
                    __instance.transform.position = replacer.itemAnchor.position + replacer.itemAnchor.forward * 0.1f;
                    return;
                }

                Transform dogTorso = replacer.GetDogGameObject().transform.Find("Armature").Find("torso");
                __instance.transform.position = dogTorso.position;
                __instance.transform.rotation = dogTorso.rotation;

            }
        }
    }
}
