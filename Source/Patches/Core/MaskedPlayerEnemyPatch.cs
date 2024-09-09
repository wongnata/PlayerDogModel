using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using UnityEngine;
using UnityEngine.Animations;

namespace PlayerDogModel_Plus.Source.Patches.Core
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemyPatch
    {
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

            // For now, let's assume the first renderer is the one we care about
            Material material = __instance.skinnedMeshRenderers[0].material;

            // Grab all the constraints for when we map the new dog body
            Transform humanPelvis = __instance.transform.Find("ScavengerModel").Find("metarig").Find("spine");
            Transform humanHead = humanPelvis.Find("spine.001").Find("spine.002").Find("spine.003").Find("spine.004");
            Transform humanLegL = humanPelvis.Find("thigh.L");
            Transform humanLegR = humanPelvis.Find("thigh.R");

            // Load in the dog model
            GameObject modelPrefab = Plugin.assetBundle.LoadAsset<GameObject>("assets/Dog.fbx");
            GameObject dogGameObject = GameObject.Instantiate(modelPrefab, __instance.transform);
            dogGameObject.transform.position = __instance.transform.position;
            dogGameObject.transform.eulerAngles = __instance.transform.eulerAngles;
            dogGameObject.transform.localScale *= 2f;

            SkinnedMeshRenderer[] dogRenderers = dogGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in dogRenderers)
            {
                renderer.material = material;
            }

            // Set up the anim correspondence with Constraints.
            Transform dogTorso = dogGameObject.transform.Find("Armature").Find("torso");
            Transform dogHead = dogTorso.Find("head");
            Transform dogArmL = dogTorso.Find("arm.L");
            Transform dogArmR = dogTorso.Find("arm.R");
            Transform dogLegL = dogTorso.Find("butt").Find("leg.L");
            Transform dogLegR = dogTorso.Find("butt").Find("leg.R");

            // Add Constraints.
            PositionConstraint torsoConstraint = dogTorso.gameObject.AddComponent<PositionConstraint>();
            torsoConstraint.AddSource(new ConstraintSource() { sourceTransform = humanPelvis, weight = 1 });
            torsoConstraint.translationAtRest = dogTorso.localPosition;
            torsoConstraint.translationOffset = dogTorso.InverseTransformPoint(humanPelvis.position);
            torsoConstraint.constraintActive = true;
            torsoConstraint.locked = true;

            // Note: the rotation offsets are not set because the model bones have the same rotation as the associated bones.
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
        }

        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(ref MaskedPlayerEnemy __instance)
        {
            if (__instance.mimickingPlayer == null) return;

            PlayerModelReplacer replacer = __instance.mimickingPlayer.GetComponent<PlayerModelReplacer>();

            if (replacer == null || !replacer.IsDog) return;

            foreach (GameObject mask in __instance.maskTypes)
            {
                mask.SetActive(false); // Indiscriminantly disabling the masks here, at least for now.
            }
        }
    }
}
