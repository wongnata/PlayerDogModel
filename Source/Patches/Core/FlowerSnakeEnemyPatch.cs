using GameNetcodeStuff;
using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(FlowerSnakeEnemy))]
    internal class FlowerSnakeEnemyPatch
    {
        [HarmonyPatch("SetClingingAnimationPosition")]
        [HarmonyPostfix]
        public static void SetClingingAnimationPositionPostfix(ref FlowerSnakeEnemy __instance, PlayerControllerB ___clingingToPlayer, int ___clingPosition, float ___spinePositionUpOffset, float ___spinePositionRightOffset)
        {
            if (___clingingToPlayer != GameNetworkManager.Instance.localPlayerController) // Only care if this is someone else
            {
                PlayerModelReplacer replacer = ___clingingToPlayer.GetComponent<PlayerModelReplacer>();

                if (replacer == null || !replacer.IsDog) return; // Nothing to do.

                // Always make this relative to the gameplay camera of the dog player
                __instance.transform.position = ___clingingToPlayer.gameplayCamera.transform.position;
                __instance.transform.position += __instance.transform.up * -0.172f;
                __instance.transform.position += __instance.transform.right * -0.013f;
                __instance.transform.rotation = ___clingingToPlayer.gameplayCamera.transform.rotation;
            }
        }
    }
}