using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
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
    }
}
