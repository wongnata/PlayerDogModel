using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using UnityEngine;
using UnityEngine.Animations;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemyPatch
    {
        private static readonly string dogComponentKey = "Dog";

        [HarmonyPatch("SetEnemyOutside")]
        [HarmonyPostfix]
        public static void SetEnemyOutsidePostfix(ref MaskedPlayerEnemy __instance)
        {
            if (__instance.mimickingPlayer == null) return;

            PlayerModelReplacer replacer = __instance.mimickingPlayer.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return;

            // Disable the renderers first
            foreach (SkinnedMeshRenderer renderer in __instance.skinnedMeshRenderers)
            {
                renderer.enabled = false;
            }
            foreach (MeshRenderer renderer in __instance.meshRenderers)
            {
                renderer.enabled = false;
            }

            Material material = __instance.skinnedMeshRenderers[0].material;

            Transform humanPelvis = __instance.transform.Find("ScavengerModel").Find("metarig").Find("spine");
            Transform humanHead = humanPelvis.Find("spine.001").Find("spine.002").Find("spine.003").Find("spine.004");
            Transform humanLegL = humanPelvis.Find("thigh.L");
            Transform humanLegR = humanPelvis.Find("thigh.R");

            GameObject modelPrefab = Plugin.assetBundle.LoadAsset<GameObject>("assets/Dog.fbx");
            GameObject dogGameObject = GameObject.Instantiate(modelPrefab, __instance.transform);

            dogGameObject.name = dogComponentKey; // Set this so we know exactly what we're looking for later
            dogGameObject.transform.position = __instance.transform.position;
            dogGameObject.transform.eulerAngles = __instance.transform.eulerAngles;
            dogGameObject.transform.localScale *= 2f;

            SkinnedMeshRenderer[] dogRenderers = dogGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in dogRenderers)
            {
                renderer.material = material;
            }

            Transform dogTorso = dogGameObject.transform.Find("Armature").Find("torso");
            Transform dogHead = dogTorso.Find("head");
            Transform dogArmL = dogTorso.Find("arm.L");
            Transform dogArmR = dogTorso.Find("arm.R");
            Transform dogLegL = dogTorso.Find("butt").Find("leg.L");
            Transform dogLegR = dogTorso.Find("butt").Find("leg.R");

            PositionConstraint torsoConstraint = dogTorso.gameObject.AddComponent<PositionConstraint>();
            torsoConstraint.AddSource(new ConstraintSource() { sourceTransform = humanPelvis, weight = 1 });
            torsoConstraint.translationAtRest = dogTorso.localPosition;
            torsoConstraint.translationOffset = dogTorso.InverseTransformPoint(humanPelvis.position);
            torsoConstraint.constraintActive = true;
            torsoConstraint.locked = true;

            RotationConstraint headConstraint = dogHead.gameObject.AddComponent<RotationConstraint>();
            headConstraint.AddSource(new ConstraintSource() { sourceTransform = humanHead, weight = 1 });
            headConstraint.rotationAtRest = dogHead.localEulerAngles;
            headConstraint.constraintActive = true;
            headConstraint.locked = true;

            RotationConstraint armLConstraint = dogArmL.gameObject.AddComponent<RotationConstraint>();
            armLConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegR, weight = 1 });
            armLConstraint.rotationAtRest = dogArmL.localEulerAngles;
            armLConstraint.constraintActive = true;
            armLConstraint.locked = true;

            RotationConstraint armRConstraint = dogArmR.gameObject.AddComponent<RotationConstraint>();
            armRConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegL, weight = 1 });
            armRConstraint.rotationAtRest = dogArmR.localEulerAngles;
            armRConstraint.constraintActive = true;
            armRConstraint.locked = true;

            RotationConstraint legLConstraint = dogLegL.gameObject.AddComponent<RotationConstraint>();
            legLConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegL, weight = 1 });
            legLConstraint.rotationAtRest = dogLegL.localEulerAngles;
            legLConstraint.constraintActive = true;
            legLConstraint.locked = true;

            RotationConstraint legRConstraint = dogLegR.gameObject.AddComponent<RotationConstraint>();
            legRConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegR, weight = 1 });
            legRConstraint.rotationAtRest = dogLegR.localEulerAngles;
            legRConstraint.constraintActive = true;
            legRConstraint.locked = true;

            // Scale up the mask objects for the dog model, since they look pretty small on their faces otherwise
            // I'm doing this for all the masks, just in case a mod might be overwriting which mask is being used
            for (int i = 0; i < __instance.maskTypes.Length; i++)
            {
                GameObject originalMask = __instance.maskTypes[i];
                GameObject maskCopy = GameObject.Instantiate(originalMask); // Copy the masks since I don't want to accidentally affect other instances
                maskCopy.transform.localScale *= 1.5f;

                __instance.maskTypes[i] = maskCopy;
            }
        }

        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(ref MaskedPlayerEnemy __instance)
        {
            if (__instance.mimickingPlayer == null) return;

            Transform dogGameObject = __instance.transform.Find(dogComponentKey);

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
    }
}
