using HarmonyLib;
using LethalNetworkAPI;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Networking;
using PlayerDogModel_Plus.Source.Util;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemyPatch
    {
        [HarmonyPatch("SetEnemyOutside")]
        [HarmonyPostfix]
        public static void SetEnemyOutsidePostfix(ref MaskedPlayerEnemy __instance)
        {
            if (RenderMaskedDog(ref __instance))
            {
                // Broadcast that this masked enemy is mimicking a dog
                MaskedDogData maskedDogData = new MaskedDogData()
                {
                    maskedEnemyNetworkId = __instance.NetworkObjectId,
                    mimickingClientId = __instance.mimickingPlayer.GetClientId()
                };

                string maskedDogString = JsonUtility.ToJson(maskedDogData);
                LNetworkMessage<string> maskedDogSpawnMessage = LNetworkMessage<string>.Connect(MessageHandler.MaskedDogSpawnMessageName);
                maskedDogSpawnMessage.SendOtherClients(maskedDogString);
                Plugin.logger.LogDebug($"Sent json={maskedDogString} for a masked dog spawn event " +
                    $"maskedEnemyNetworkId={maskedDogData.maskedEnemyNetworkId}, " +
                    $"mimickingClientId={maskedDogData.mimickingClientId}");
            }
        }

        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(ref MaskedPlayerEnemy __instance)
        {
            if (__instance.mimickingPlayer == null) return;

            Transform dogGameObject = __instance.transform.Find(DogModelMapper.dogComponentKey);

            if (dogGameObject == null) return; // Wasn't mimicking a dog

            GameObject mask = __instance.maskTypes[__instance.maskTypeIndex];

            if (Plugin.isMirageLoaded || Plugin.config.alwaysHideMasksOnDogs.Value)
            {
                mask.gameObject.SetActive(false);
                return; // Don't bother adjusting position
            }

            Transform dogHead = dogGameObject.Find("Armature").Find("torso").Find("head");
            mask.transform.rotation = dogHead.rotation;
            mask.transform.position = dogHead.position + dogHead.forward * 0.5f + dogHead.up * 0.2f;
            mask.transform.Rotate(-37, 0, 0);
            mask.gameObject.SetActive(true);
        }

        internal static bool RenderMaskedDog(ref MaskedPlayerEnemy mimic)
        {
            if (mimic.mimickingPlayer == null) return false;

            PlayerModelReplacer replacer = mimic.mimickingPlayer.GetComponent<PlayerModelReplacer>();

            // Wasn't a dog or already had a dog rendered
            if (replacer == null || !replacer.IsDog || (mimic.transform.Find(DogModelMapper.dogComponentKey) != null)) return false;

            DisableMaskedRenderers(mimic); ;

            GameObject modelPrefab = Plugin.assetBundle.LoadAsset<GameObject>("assets/Dog.fbx");
            GameObject dogGameObject = GameObject.Instantiate(modelPrefab, mimic.transform);

            SkinnedMeshRenderer[] dogRenderers = dogGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            Material material = StartOfRound.Instance.unlockablesList.unlockables[mimic.mimickingPlayer.currentSuitID].suitMaterial;
            foreach (SkinnedMeshRenderer renderer in dogRenderers)
            {
                renderer.material = material;
            }

            DogModelMapper.MapDogModelToHumanModel(dogGameObject, mimic.transform);

            // Scale up the mask objects for the dog model, since they look pretty small on their faces otherwise
            // I'm doing this for all the masks, just in case a mod might be overwriting which mask is being used
            for (int i = 0; i < mimic.maskTypes.Length; i++)
            {
                GameObject originalMask = mimic.maskTypes[i];
                GameObject maskCopy = GameObject.Instantiate(originalMask); // Copy the masks since I don't want to accidentally affect other instances
                maskCopy.transform.localScale *= 1.5f;

                mimic.maskTypes[i] = maskCopy;
            }

            return true;
        }

        private static void DisableMaskedRenderers(MaskedPlayerEnemy mimic)
        {
            // Sometimes it's possible for this to get called before the renders have been set.
            if (mimic.skinnedMeshRenderers.Length == 0)
            {
                mimic.skinnedMeshRenderers = mimic.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            }
            if (mimic.meshRenderers.Length == 0)
            {
                mimic.meshRenderers = mimic.gameObject.GetComponentsInChildren<MeshRenderer>();
            }

            // Disable the renderers so the original model is hidden
            foreach (SkinnedMeshRenderer renderer in mimic.skinnedMeshRenderers)
            {
                renderer.enabled = false;
            }
            foreach (MeshRenderer renderer in mimic.meshRenderers)
            {
                renderer.enabled = false;
            }
        }
    }
}
