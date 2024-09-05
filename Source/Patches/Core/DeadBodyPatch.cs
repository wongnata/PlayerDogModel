using HarmonyLib;
using PlayerDogModel_Plus.Source.Model;
using UnityEngine;
using UnityEngine.Animations;

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

            Transform humanPelvis = __instance.transform.Find("spine.001");
            Transform humanHead = humanPelvis.Find("spine.002").Find("spine.003").Find("spine.004");
            Transform humanArmL = humanPelvis.Find("spine.002").Find("spine.003").Find("shoulder.L").Find("arm.L_upper");
            Transform humanArmR = humanPelvis.Find("spine.002").Find("spine.003").Find("shoulder.R").Find("arm.R_upper");
            Transform humanLegL = humanPelvis.Find("thigh.L");
            Transform humanLegR = humanPelvis.Find("thigh.R");

            // Load and spawn new model.
            GameObject modelPrefab = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<GameObject>("assets/DogRagdoll.fbx");
            GameObject dogGameObject = Object.Instantiate(modelPrefab, __instance.transform);
            dogGameObject.transform.position = __instance.transform.position;
            dogGameObject.transform.eulerAngles = __instance.transform.eulerAngles;
            dogGameObject.transform.localScale *= 1.8f;

            // Copy the material. Note: this is also changed in the Update.
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
            // Note: the rotation offsets are not set because the model bones have the same rotation as the associated bones.
            RotationConstraint torsoConstraint = dogTorso.gameObject.AddComponent<RotationConstraint>();
            torsoConstraint.AddSource(new ConstraintSource() { sourceTransform = humanPelvis, weight = 0.5f });
            torsoConstraint.rotationAtRest = dogHead.localEulerAngles;
            torsoConstraint.constraintActive = true;
            torsoConstraint.locked = true;

            RotationConstraint headConstraint = dogHead.gameObject.AddComponent<RotationConstraint>();
            headConstraint.AddSource(new ConstraintSource() { sourceTransform = humanHead, weight = 0.5f });
            headConstraint.rotationAtRest = dogHead.localEulerAngles;
            headConstraint.constraintActive = true;
            headConstraint.locked = true;

            RotationConstraint armLConstraint = dogArmL.gameObject.AddComponent<RotationConstraint>();
            armLConstraint.AddSource(new ConstraintSource() { sourceTransform = humanArmR, weight = 0.5f });
            armLConstraint.rotationAtRest = dogArmL.localEulerAngles;
            armLConstraint.constraintActive = true;
            armLConstraint.locked = true;

            RotationConstraint armRConstraint = dogArmR.gameObject.AddComponent<RotationConstraint>();
            armRConstraint.AddSource(new ConstraintSource() { sourceTransform = humanArmL, weight = 0.5f });
            armRConstraint.rotationAtRest = dogArmR.localEulerAngles;
            armRConstraint.constraintActive = true;
            armRConstraint.locked = true;

            RotationConstraint legLConstraint = dogLegL.gameObject.AddComponent<RotationConstraint>();
            legLConstraint.AddSource(new ConstraintSource() { sourceTransform = humanArmL, weight = 0.5f });
            legLConstraint.rotationAtRest = dogLegL.localEulerAngles;
            legLConstraint.constraintActive = true;
            legLConstraint.locked = true;

            RotationConstraint legRConstraint = dogLegR.gameObject.AddComponent<RotationConstraint>();
            legRConstraint.AddSource(new ConstraintSource() { sourceTransform = humanArmR, weight = 0.5f });
            legRConstraint.rotationAtRest = dogLegR.localEulerAngles;
            legRConstraint.constraintActive = true;
            legRConstraint.locked = true;
        }
    }
}
