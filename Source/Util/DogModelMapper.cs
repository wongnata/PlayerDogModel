using UnityEngine;
using UnityEngine.Animations;

namespace PlayerDogModel_Plus.Source.Util
{
    internal class DogModelMapper
    {
        internal static readonly string dogComponentKey = "Dog";

        public static void MapDogModelToHumanModel(GameObject dogGameObject, Transform humanModel)
        {
            dogGameObject.name = dogComponentKey; // Set this so we know exactly what we're looking for later
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
            MapPositionConstraintToSource(torsoConstraint, humanPelvis, dogTorso.localPosition, dogTorso.InverseTransformPoint(humanPelvis.position));

            RotationConstraint headConstraint = dogHead.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(headConstraint, humanHead, dogHead.localEulerAngles);

            RotationConstraint armLConstraint = dogArmL.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(armLConstraint, humanLegR, dogArmL.localEulerAngles);

            RotationConstraint armRConstraint = dogArmR.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(armRConstraint, humanLegL, dogArmR.localEulerAngles);

            RotationConstraint legLConstraint = dogLegL.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(legLConstraint, humanLegL, dogLegL.localEulerAngles);

            RotationConstraint legRConstraint = dogLegR.gameObject.AddComponent<RotationConstraint>();
            MapRotationConstraintToSource(legRConstraint, humanLegR, dogLegR.localEulerAngles);
        }

        private static void MapPositionConstraintToSource(PositionConstraint dogConstraint, Transform humanSource, Vector3 translationAtRest, Vector3 translationOffset)
        {
            dogConstraint.AddSource(new ConstraintSource() { sourceTransform = humanSource, weight = 1 });
            dogConstraint.translationAtRest = translationAtRest;
            dogConstraint.translationOffset = translationOffset;
            dogConstraint.constraintActive = true;
            dogConstraint.locked = true;
        }

        private static void MapRotationConstraintToSource(RotationConstraint dogConstraint, Transform humanSource, Vector3 rotationAtRest)
        {
            dogConstraint.AddSource(new ConstraintSource() { sourceTransform = humanSource, weight = 1 });
            dogConstraint.rotationAtRest = rotationAtRest;
            dogConstraint.constraintActive = true;
            dogConstraint.locked = true;
        }
    }
}
