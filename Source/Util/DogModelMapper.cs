using UnityEngine;
using UnityEngine.Animations;

namespace PlayerDogModel_Plus.Source.Util
{
    internal class DogModelMapper
    {
        internal static readonly string dogModelKey = "DogModel";
        internal static readonly string dogRagdollKey = "DogRagdoll";

        public static DogModelConstraints MapDogModelToHumanModel(GameObject dogGameObject, Transform humanModel)
        {
            dogGameObject.name = dogModelKey; // Set this for lookups
            dogGameObject.transform.position = humanModel.position;
            dogGameObject.transform.eulerAngles = humanModel.eulerAngles;
            dogGameObject.transform.localScale *= 2f;

            Transform dogTorso = dogGameObject.transform.Find("Armature").Find("torso");
            Transform dogHead = dogTorso.Find("head");
            Transform dogArmL = dogTorso.Find("arm.L");
            Transform dogArmR = dogTorso.Find("arm.R");
            Transform dogLegL = dogTorso.Find("butt").Find("leg.L");
            Transform dogLegR = dogTorso.Find("butt").Find("leg.R");

            Transform humanPelvis = humanModel.Find("ScavengerModel").Find("metarig").Find("spine");
            Transform humanHead = humanPelvis.Find("spine.001").Find("spine.002").Find("spine.003").Find("spine.004");
            Transform humanLegL = humanPelvis.Find("thigh.L");
            Transform humanLegR = humanPelvis.Find("thigh.R");

            PositionConstraint torsoConstraint = dogTorso.gameObject.AddComponent<PositionConstraint>();
            MapPositionConstraintToSource(torsoConstraint, humanPelvis, dogTorso.localPosition, dogTorso.InverseTransformPoint(humanPelvis.position), 1);

            RotationConstraint headConstraint = dogHead.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(headConstraint, humanHead, dogHead.localEulerAngles, 1);

            RotationConstraint armLConstraint = dogArmL.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(armLConstraint, humanLegR, dogArmL.localEulerAngles, 1);

            RotationConstraint armRConstraint = dogArmR.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(armRConstraint, humanLegL, dogArmR.localEulerAngles, 1);

            RotationConstraint legLConstraint = dogLegL.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(legLConstraint, humanLegL, dogLegL.localEulerAngles, 1);

            RotationConstraint legRConstraint = dogLegR.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(legRConstraint, humanLegR, dogLegR.localEulerAngles, 1);

            return new DogModelConstraints()
            {
                torso = torsoConstraint,
                head = headConstraint,
                armL = armLConstraint,
                armR = armRConstraint,
                legL = legLConstraint,
                legR = legRConstraint
            };
        }

        public static void MapDogRagdollToHumanRagdoll(GameObject dogGameObject, Transform humanRagdoll)
        {
            dogGameObject.name = dogRagdollKey; // Set this for lookups
            dogGameObject.transform.position = humanRagdoll.position;
            dogGameObject.transform.eulerAngles = humanRagdoll.eulerAngles;
            dogGameObject.transform.localScale *= 1.8f;

            Transform dogTorso = dogGameObject.transform.Find("Armature").Find("torso");
            Transform dogHead = dogTorso.Find("head");
            Transform dogArmL = dogTorso.Find("arm.L");
            Transform dogArmR = dogTorso.Find("arm.R");
            Transform dogLegL = dogTorso.Find("butt").Find("leg.L");
            Transform dogLegR = dogTorso.Find("butt").Find("leg.R");

            Transform humanPelvis = humanRagdoll.Find("spine.001");
            Transform humanHead = humanPelvis.Find("spine.002").Find("spine.003").Find("spine.004");
            Transform humanArmL = humanPelvis.Find("spine.002").Find("spine.003").Find("shoulder.L").Find("arm.L_upper");
            Transform humanArmR = humanPelvis.Find("spine.002").Find("spine.003").Find("shoulder.R").Find("arm.R_upper");

            RotationConstraint torsoConstraint = dogTorso.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(torsoConstraint, humanPelvis, dogTorso.localEulerAngles, 0.5f);

            RotationConstraint headConstraint = dogHead.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(headConstraint, humanHead, dogHead.localEulerAngles, 0.5f);

            RotationConstraint armLConstraint = dogArmL.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(armLConstraint, humanArmR, dogArmL.localEulerAngles, 0.5f);

            RotationConstraint armRConstraint = dogArmR.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(armRConstraint, humanArmL, dogArmR.localEulerAngles, 0.5f);

            RotationConstraint legLConstraint = dogLegL.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(legLConstraint, humanArmL, dogLegL.localEulerAngles, 0.5f);

            RotationConstraint legRConstraint = dogLegR.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(legRConstraint, humanArmR, dogLegR.localEulerAngles, 0.5f);
        }

        private static void MapPositionConstraintToSource(PositionConstraint dogConstraint, Transform humanSource, Vector3 translationAtRest, Vector3 translationOffset, float weight)
        {
            dogConstraint.AddSource(new ConstraintSource() { sourceTransform = humanSource, weight = weight });
            dogConstraint.translationAtRest = translationAtRest;
            dogConstraint.translationOffset = translationOffset;
            dogConstraint.constraintActive = true;
            dogConstraint.locked = true;
        }

        private static void MapRotationConstraintToSource(RotationConstraint dogConstraint, Transform humanSource, Vector3 rotationAtRest, float weight)
        {
            dogConstraint.AddSource(new ConstraintSource() { sourceTransform = humanSource, weight = weight });
            dogConstraint.rotationAtRest = rotationAtRest;
            dogConstraint.constraintActive = true;
            dogConstraint.locked = true;
        }
    }
}
