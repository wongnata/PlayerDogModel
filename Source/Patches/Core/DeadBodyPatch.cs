using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Util;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    // Ragdoll support! It's whacky who cares.
    [HarmonyPatch(typeof(DeadBodyInfo))]
    internal class DeadBodyPatch
    {
        // Start is called when the ragdoll is instantiated.
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix(ref DeadBodyInfo __instance)
        {
            if (!__instance.playerScript.GetComponent<PlayerModelReplacer>().IsDog)
            {
                return;
            }

            // No need to add a new component: just hide the human if relevant and spawn the mesh.
            SkinnedMeshRenderer humanRenderer = __instance.GetComponent<SkinnedMeshRenderer>();
            humanRenderer.enabled = false;
            Material material = humanRenderer.material;

            // Load and spawn new model.
            GameObject modelPrefab = Plugin.assetBundle.LoadAsset<GameObject>("assets/DogRagdoll.fbx");
            GameObject dogGameObject = Object.Instantiate(modelPrefab, __instance.transform);

            // Copy the material. Note: this is also changed in the Update.
            SkinnedMeshRenderer[] dogRenderers = dogGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in dogRenderers)
            {
                renderer.material = material;
            }

            DogModelMapper.MapDogRagdollToHumanRagdoll(dogGameObject, __instance.transform);
        }
    }
}
