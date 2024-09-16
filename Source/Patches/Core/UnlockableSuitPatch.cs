using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    // This allows to wear other suits.
    [HarmonyPatch(typeof(UnlockableSuit))]
    internal class UnlockableSuitPatch
    {
        // SwitchSuitForPlayer is called when switching to a new suit.
        [HarmonyPatch("SwitchSuitForPlayer")]
        [HarmonyPostfix]
        public static void SwitchSuitForPlayerPostfix(PlayerControllerB player, int suitID, bool playAudio = true)
        {
            PlayerModelReplacer replacer = player.GetComponent<PlayerModelReplacer>();
            if (replacer)
            {
                replacer.UpdateMaterial();
            }

            if (replacer != null && replacer.IsDog)
            {
                // Hide the head cosmetics for dog players
                if (GameNetworkManager.Instance.localPlayerController != player)
                {
                    UnlockableSuit.ChangePlayerCostumeElement(player.headCostumeContainer, null);
                }
                else
                {
                    UnlockableSuit.ChangePlayerCostumeElement(player.headCostumeContainerLocal, null);
                }
            }
        }
    }
}
