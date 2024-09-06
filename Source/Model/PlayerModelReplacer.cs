using BepInEx.Bootstrap;
using GameNetcodeStuff;
using LethalNetworkAPI;
using PlayerDogModel_Plus.Source.Patches.Optional;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace PlayerDogModel_Plus.Source.Model
{
    // By default, LateUpdate is called in a chaotic order: GrabbableObject can execute it before or after PlayerModelReplacer.
    // Forcing the Execution Order to this value will ensure PlayerModelReplacer updates the anchor first and THEN only the GrabbableObject will update its position.
    [DefaultExecutionOrder(-1)]
    public class PlayerModelReplacer : MonoBehaviour
    {
        public static PlayerModelReplacer LocalReplacer;

        public ulong PlayerClientId => playerController != null ? playerController.playerClientId : 0xffff_ffff_ffff_fffful;
        public string PlayerUsername => playerController != null ? playerController.playerUsername : "";
        public bool IsValid => dogGameObject != null;

        private static bool loaded;
        private static string exceptionMessage;
        private static System.Exception exception;

        private PlayerControllerB playerController;
        private GameObject dogGameObject;
        private GameObject[] humanGameObjects;
        private SkinnedMeshRenderer[] dogRenderers;

        private Transform dogTorso;
        private PositionConstraint torsoConstraint;

        private static AudioClip humanClip, dogClip;

        private Vector3 humanCameraPosition;
        public Transform itemAnchor;

        private static Image healthFill, healthOutline;
        private static Sprite humanFill, humanOutline, dogFill, dogOutline;

        private bool isDogActive;

        public bool IsDog
        {
            get
            {
                return isDogActive;
            }
        }

        public Transform GetDogTorso()
        {
            return dogTorso;
        }

        public GameObject GetDogGameObject()
        {
            return dogGameObject;
        }

        private void Awake()
        {
            if (!loaded)
            {
                loaded = true;
                LoadImageResources();
                StartCoroutine(LoadAudioResources());
            }
        }

        private void Start()
        {
            playerController = GetComponent<PlayerControllerB>();

            if (playerController.IsOwner)
            {
                LocalReplacer = this;
            }

            humanCameraPosition = playerController.gameplayCamera.transform.localPosition;

            Plugin.logger.LogDebug($"Adding PlayerModelReplacer on {playerController.playerUsername} ({playerController.IsOwner})");

            SpawnDogModel();
            EnableHumanModel(false);
        }

        private void Update()
        {
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                return;
            }

            // Adjust camera height.
            Vector3 cameraPositionGoal = humanCameraPosition;
            if (isDogActive && !playerController.inTerminalMenu && !playerController.inSpecialInteractAnimation)
            {
                if (!playerController.isCrouching)
                {
                    cameraPositionGoal = new Vector3(0, -0.8f, 0.3f);
                }
                else
                {
                    cameraPositionGoal = new Vector3(0, -0.1f, 0.3f);
                }
            }

            playerController.gameplayCamera.transform.localPosition = Vector3.MoveTowards(playerController.gameplayCamera.transform.localPosition, cameraPositionGoal, Time.deltaTime * 2);

            // Adjust position constraint to avoid going through the floor.
            torsoConstraint.weight = Mathf.MoveTowards(torsoConstraint.weight, 0.5f, Time.deltaTime * 3);

            // Adjust torso rotation for climbing animation.
            if (playerController.isClimbingLadder)
            {
                dogTorso.localRotation = Quaternion.RotateTowards(dogTorso.localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * 360);
            }
            else
            {
                dogTorso.localRotation = Quaternion.RotateTowards(dogTorso.localRotation, Quaternion.Euler(180, 0, 0), Time.deltaTime * 360);
            }
        }

        private void LateUpdate()
        {
            if (itemAnchor == null)
            {
                return;
            }

            // Update the location of the item anchor. This is reset by animation between every Update and LateUpate.
            // Thanks to the DefaultExecutionOrder attribute we know it'll be executed BEFORE the GrabbableObject.LateUpdate().
            if (isDogActive)
            {
                playerController.localItemHolder.position = itemAnchor.position;
                playerController.serverItemHolder.position = itemAnchor.position;
            }

            // Make sure the shadow casting mode and layer are right despite other mods.
            if (dogRenderers[0].shadowCastingMode != playerController.thisPlayerModel.shadowCastingMode)
            {
                Plugin.logger.LogDebug($"Dog model is on the wrong shadow casting mode. ({dogRenderers[0].shadowCastingMode} instead of {playerController.thisPlayerModel.shadowCastingMode})");
                dogRenderers[0].shadowCastingMode = playerController.thisPlayerModel.shadowCastingMode;
            }

            if (dogRenderers[0].gameObject.layer != playerController.thisPlayerModel.gameObject.layer)
            {
                Plugin.logger.LogDebug($"Dog model is on the wrong layer. ({LayerMask.LayerToName(dogRenderers[0].gameObject.layer)} instead of {LayerMask.LayerToName(playerController.thisPlayerModel.gameObject.layer)})");
                dogRenderers[0].gameObject.layer = playerController.thisPlayerModel.gameObject.layer;
            }
        }

        private void SpawnDogModel()
        {
            try
            {
                // Load and spawn new model.
                GameObject modelPrefab = Plugin.assetBundle.LoadAsset<GameObject>("assets/Dog.fbx");
                dogGameObject = Instantiate(modelPrefab, transform);
                dogGameObject.transform.position = transform.position;
                dogGameObject.transform.eulerAngles = transform.eulerAngles;
                dogGameObject.transform.localScale *= 2f;
            }
            catch (System.Exception e)
            {
                exceptionMessage = "Failed to spawn dog model.";
                exception = e;

                Plugin.logger.LogError(exceptionMessage);
            }

            // Copy the material. Note: this is also changed in the Update.
            dogRenderers = dogGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            UpdateMaterial();

            try
            {
                // Enable LOD. This is both for performances and being visible in cameras. Values are simply copied from the human LOD.
                LODGroup lodGroup = dogGameObject.AddComponent<LODGroup>();
                lodGroup.fadeMode = LODFadeMode.None;
                LOD lod1 = new LOD() { screenRelativeTransitionHeight = 0.4564583f, renderers = new Renderer[] { dogRenderers[0] }, fadeTransitionWidth = 0f };
                LOD lod2 = new LOD() { screenRelativeTransitionHeight = 0.1795709f, renderers = new Renderer[] { dogRenderers[1] }, fadeTransitionWidth = 0f };
                LOD lod3 = new LOD() { screenRelativeTransitionHeight = 0.009000001f, renderers = new Renderer[] { dogRenderers[2] }, fadeTransitionWidth = 0.435f };
                lodGroup.SetLODs(new LOD[] { lod1, lod2, lod3 });
            }
            catch (System.Exception e)
            {
                exceptionMessage = "Failed to set up the LOD.";
                exception = e;

                Plugin.logger.LogError(exceptionMessage);
            }

            try
            {
                // Set up the anim correspondence with Constraints.
                dogTorso = dogGameObject.transform.Find("Armature").Find("torso");
                Transform dogHead = dogTorso.Find("head");
                Transform dogArmL = dogTorso.Find("arm.L");
                Transform dogArmR = dogTorso.Find("arm.R");
                Transform dogLegL = dogTorso.Find("butt").Find("leg.L");
                Transform dogLegR = dogTorso.Find("butt").Find("leg.R");

                Transform humanPelvis = transform.Find("ScavengerModel").Find("metarig").Find("spine");
                Transform humanHead = humanPelvis.Find("spine.001").Find("spine.002").Find("spine.003").Find("spine.004");
                Transform humanLegL = humanPelvis.Find("thigh.L");
                Transform humanLegR = humanPelvis.Find("thigh.R");

                try
                {
                    // Add Constraints.
                    torsoConstraint = dogTorso.gameObject.AddComponent<PositionConstraint>();
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
                catch (System.Exception e)
                {
                    exceptionMessage = "Failed to set up the constraints.";
                    exception = e;

                    Plugin.logger.LogError(exceptionMessage);
                }

                // Fetch the anchor for the items.
                itemAnchor = dogHead.Find("serverItem");
            }
            catch (System.Exception e)
            {
                exceptionMessage = "Failed to retrieve bones. What the hell?";
                exception = e;

                Plugin.logger.LogError(exceptionMessage);
            }

            // Get a handy list of gameobjects to disable.
            humanGameObjects = new GameObject[6];
            humanGameObjects[0] = playerController.thisPlayerModel.gameObject;
            humanGameObjects[1] = playerController.thisPlayerModelLOD1.gameObject;
            humanGameObjects[2] = playerController.thisPlayerModelLOD2.gameObject;
            humanGameObjects[3] = playerController.thisPlayerModelArms.gameObject;
            humanGameObjects[4] = playerController.playerBetaBadgeMesh.gameObject;
            humanGameObjects[5] = playerController.playerBetaBadgeMesh.transform.parent.Find("LevelSticker").gameObject;
        }

        public void EnableHumanModel(bool playAudio)
        {
            isDogActive = false;

            // Dog can be completely disabled because it doesn't drive the animations and sounds and other stuff.
            dogGameObject.SetActive(false);

            // Human renderers have to be directly disabled: the game object contains the camera and stuff and must remain enabled.
            foreach (GameObject humanGameObject in humanGameObjects)
            {
                humanGameObject.SetActive(true);
            }

            if (playAudio)
            {
                playerController.movementAudio.PlayOneShot(humanClip);
            }

            if (playerController.IsOwner)
            {
                if (healthFill)
                {
                    healthFill.sprite = humanFill;
                    healthOutline.sprite = humanOutline;
                }
            }
        }

        public void EnableDogModel(bool playAudio)
        {
            isDogActive = true;

            dogGameObject.SetActive(true);

            // Make sure the shadow casting mode is the same. Still don't know how the player is visible in cameras but not in first person. It's not a layer thing, maybe it's the LOD?
            dogRenderers[0].shadowCastingMode = playerController.thisPlayerModel.shadowCastingMode;

            foreach (GameObject humanGameObject in humanGameObjects)
            {
                humanGameObject.SetActive(false);
            }

            if (playAudio)
            {
                playerController.movementAudio.PlayOneShot(dogClip);
            }

            if (playerController.IsOwner)
            {
                if (!healthFill)
                {
                    healthFill = HUDManager.Instance.selfRedCanvasGroup.GetComponent<Image>();
                    healthOutline = HUDManager.Instance.selfRedCanvasGroup.transform.parent.Find("Self").GetComponent<Image>();

                    humanFill = healthFill.sprite;
                    humanOutline = healthOutline.sprite;
                }

                healthFill.sprite = dogFill;
                healthOutline.sprite = dogOutline;
            }
        }

        public void UpdateMaterial()
        {
            if (dogRenderers == null)
            {
                Plugin.logger.LogWarning($"Skipping material replacement on dog because there was an error earlier.");
                return;
            }

            foreach (Renderer renderer in dogRenderers)
            {
                renderer.material = playerController.thisPlayerModel.material;
            }
        }

        public void ToggleAndBroadcast(bool playAudio)
        {
            Plugin.logger.LogDebug($"Toggling dog mode for you ({playerController.playerUsername})!");
            if (isDogActive)
            {
                EnableHumanModel(playAudio);
            }
            else
            {
                EnableDogModel(playAudio);
            }

            BroadcastSelectedModel(playAudio);
        }

        public void ReceiveBroadcastAndToggle(bool playAudio, bool isDog)
        {
            if (isDog)
            {
                Plugin.logger.LogDebug($"Turning {playerController.playerUsername} into a dog! Woof!");
                EnableDogModel(playAudio);

                if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
                {
                    MoreCompanyPatch.HideCosmeticsForPlayer(playerController);
                }
            }
            else
            {
                Plugin.logger.LogDebug($"Turning {playerController.playerUsername} into a human!");
                EnableHumanModel(playAudio);

                if (playerController.IsOwner) // This should only be true once when you start up!
                {
                    Plugin.logger.LogDebug($"Hang on, you're {playerController.playerUsername}, we won't show your cosmetics!");

                    if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
                    {
                        MoreCompanyPatch.HideCosmeticsForPlayer(playerController);
                    }
                    return;
                }
                else
                {
                    if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
                    {
                        MoreCompanyPatch.ShowCosmeticsForPlayer(playerController);
                    }
                }
            }
        }

        public void BroadcastSelectedModel(bool playAudio)
        {
            Plugin.logger.LogDebug($"Sent dog={isDogActive} on {playerController.playerClientId} ({playerController.playerUsername}).");
            LethalClientMessage<bool> selectedModelMessage = new LethalClientMessage<bool>(Networking.ModelSwitchMessageName);
            selectedModelMessage.SendAllClients(isDogActive, false);
        }

        private static void LoadImageResources()
        {
            try
            {
                Texture2D filled = Plugin.assetBundle.LoadAsset<Texture2D>("assets/TPoseFilled.png");
                dogFill = Sprite.Create(filled, new Rect(0, 0, filled.width, filled.height), new Vector2(0.5f, 0.5f), 100f);

                Texture2D outline = Plugin.assetBundle.LoadAsset<Texture2D>("assets/TPoseOutline.png");
                dogOutline = Sprite.Create(outline, new Rect(0, 0, outline.width, outline.height), new Vector2(0.5f, 0.5f), 100f);
            }
            catch (System.Exception e)
            {
                exceptionMessage = "Failed to retrieve images.";
                exception = e;

                Plugin.logger.LogError(exceptionMessage);
            }
        }

        private static IEnumerator LoadAudioResources()
        {
            string fullPath = GetAssemblyFullPath("ChangeSuitToHuman.wav");
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
            yield return request.SendWebRequest();
            if (request.error == null)
            {
                humanClip = DownloadHandlerAudioClip.GetContent(request);
                humanClip.name = Path.GetFileName(fullPath);
            }

            fullPath = GetAssemblyFullPath("ChangeSuitToDog.wav");
            request = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
            yield return request.SendWebRequest();
            if (request.error == null)
            {
                dogClip = DownloadHandlerAudioClip.GetContent(request);
                dogClip.name = Path.GetFileName(fullPath);
            }
        }

        private static string GetAssemblyFullPath(string additionalPath)
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = additionalPath != null ? Path.Combine(directoryName, ".\\" + additionalPath) : directoryName;
            return Path.GetFullPath(path);
        }
    }
}

