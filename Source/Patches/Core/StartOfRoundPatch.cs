using HarmonyLib;
using LethalNetworkAPI;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Util;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    // PlayerModelSwitcher is the interaction which allows the player to toggle the model on and off.
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        // PositionSuitsOnRack is called when the game scene is prepared.
        [HarmonyPatch("PositionSuitsOnRack")]
        [HarmonyPostfix]
        public static void PositionSuitsOnRackPostfix(ref StartOfRound __instance)
        {
            PlayerModelSwitcher switcher = Object.FindObjectOfType<PlayerModelSwitcher>();
            if (!switcher)
            {
                // This weird name is the suits hanger.
                GameObject suitHanger = GameObject.Find("NurbsPath.002");
                suitHanger.AddComponent<PlayerModelSwitcher>();
            }
        }

        [HarmonyPatch("OnClientDisconnect")]
        [HarmonyPrefix]
        public static void OnClientDisconnectPostfix(ref ulong clientId)
        {
            // Reset the status of this player in case they reconnect
            clientId.GetPlayerController().GetComponent<PlayerModelReplacer>().EnableHumanModel(false);

            // Remove this client from the caches
            ModelReplacerRetriever.OnClientDisconnect(clientId);
            PlayerRetriever.OnClientDisconnect(clientId);
        }
    }
}
