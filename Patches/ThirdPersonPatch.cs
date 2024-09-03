using _3rdPerson.Helper;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using static PlayerActions;

namespace PlayerDogModel_Plus.Patches
{
    internal class ThirdPersonPatch
    {
        [HarmonyPatch(typeof(ThirdPersonCamera))]
        [HarmonyPatch("ThirdPersonUpdate")]
        class ThirdPersonUpdatePatch
        {
            static bool Prefix(ref Camera ____camera)
            {
                if (!Plugin.boundConfig.thirdPersonConfigOverride.Value) return true; // Skip if we're not overriding.

                PlayerControllerB playerController = LocalPlayer.GetController();
                Camera gameplayCamera = playerController.gameplayCamera;

                PlayerModelReplacer replacer = null;
                foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
                {
                    var currentReplacer = player.GetComponent<PlayerModelReplacer>();
                    if (currentReplacer != null && currentReplacer.PlayerClientId == playerController.playerClientId)
                    {
                        replacer = currentReplacer;
                        break;
                    }
                }

                if (replacer == null || !replacer.IsDog) return true;

                // Rest of this is ripped directly from the original method, minus the custom configs.
                Vector3 forwardVector = gameplayCamera.transform.forward * -1f;
                Vector3 rightVector = gameplayCamera.transform.TransformDirection(Vector3.right) * Plugin.boundConfig.thirdPersonRightOffset.Value;
                Vector3 upVector = Vector3.up * Plugin.boundConfig.thirdPersonUpOffset.Value;

                float distance = Plugin.boundConfig.thirdPersonDistance.Value;

                ____camera.transform.position = gameplayCamera.transform.position + forwardVector * distance + rightVector + upVector;
                ____camera.transform.rotation = Quaternion.LookRotation(gameplayCamera.transform.forward);
                
                return false;
            }
        }

        [HarmonyPatch(typeof(ThirdPersonCamera))]
        [HarmonyPatch("ThirdPersonOrbitUpdate")]
        class ThirdPersonOrbitUpdatePatch
        {
            static bool Prefix(ref Camera ____camera)
            {
                if (!Plugin.boundConfig.thirdPersonConfigOverride.Value) return true; // Skip if we're not overriding.

                PlayerControllerB playerController = LocalPlayer.GetController();
                Camera gameplayCamera = playerController.gameplayCamera;

                PlayerModelReplacer replacer = null;
                foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
                {
                    var currentReplacer = player.GetComponent<PlayerModelReplacer>();
                    if (currentReplacer != null && currentReplacer.PlayerClientId == playerController.playerClientId)
                    {
                        replacer = currentReplacer;
                        break;
                    }
                }

                if (replacer == null || !replacer.IsDog) return true;

                MovementActions movement = playerController.playerActions.Movement;
                Vector2 lookVector = movement.Look.ReadValue<Vector2>() * (0.008f * (float)IngamePlayerSettings.Instance.settings.lookSensitivity);

                if (IngamePlayerSettings.Instance.settings.invertYAxis)
                {
                    lookVector.y *= -1f;
                }

                // Rest of this is ripped directly from the original method, minus the custom configs.
                ____camera.transform.Rotate(Vector3.right, lookVector.y * _3rdPerson.Plugin.OrbitSpeedEntry.Value * Time.deltaTime);
                ____camera.transform.RotateAround((playerController.gameplayCamera).transform.position, Vector3.up, lookVector.x * _3rdPerson.Plugin.OrbitSpeedEntry.Value * Time.deltaTime);
                float distance = Plugin.boundConfig.thirdPersonDistance.Value;
                ____camera.transform.position = (playerController.gameplayCamera).transform.position - ____camera.transform.forward * distance;

                return false;
            }
        }
    }
}
