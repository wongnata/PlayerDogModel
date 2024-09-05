using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace PlayerDogModel_Plus.Patches.Core
{
    // PlayerModelReplacer handles the model and its toggling.
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        // SpawnPlayerAnimation is called when respawning.
        [HarmonyPatch("SpawnPlayerAnimation")]
        [HarmonyPostfix]
        public static void SpawnPlayerAnimationPatch(ref PlayerControllerB __instance)
        {
            // Find all the players and add the script to them if they don't have it yet.
            // This is done for every player every time a player spawns just to be sure.
            foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
            {
                if (!player.GetComponent<PlayerModelReplacer>())
                {
                    player.gameObject.AddComponent<PlayerModelReplacer>();
                }
            }

            // Request data regarding the other players' skins.
            PlayerModelReplacer.RequestSelectedModelBroadcast();
        }
    }
}
