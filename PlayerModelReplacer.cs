using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using UnityEngine.Networking;
using System.IO;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;
using BepInEx.Bootstrap;
using PlayerDogModel_Plus.Patches;

namespace PlayerDogModel_Plus
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
		private Transform localItemAnchor, serverItemAnchor;

		private static Image healthFill, healthOutline;
		private static Sprite humanFill, humanOutline, dogFill, dogOutline;

		private bool isDogActive;

		public bool IsDog
		{
			get
			{
				return this.isDogActive;
			}
		}

		private void Awake()
		{
			if (!PlayerModelReplacer.loaded)
			{
				PlayerModelReplacer.loaded = true;
				PlayerModelReplacer.LoadImageResources();
				this.StartCoroutine(PlayerModelReplacer.LoadAudioResources());
			}
		}

		private void Start()
		{
			this.playerController = this.GetComponent<PlayerControllerB>();

			if (this.playerController.IsOwner)
			{
				PlayerModelReplacer.LocalReplacer = this;
			}

			this.humanCameraPosition = this.playerController.gameplayCamera.transform.localPosition;

			Debug.Log($"Adding PlayerModelReplacer on {this.playerController.playerUsername} ({this.playerController.IsOwner})");

			this.SpawnDogModel();
			this.EnableHumanModel(false);
		}

		private void Update()
		{
			if (!string.IsNullOrEmpty(PlayerModelReplacer.exceptionMessage))
			{
				return;
			}

			// Adjust camera height.
			Vector3 cameraPositionGoal = this.humanCameraPosition;
			if (this.isDogActive && !this.playerController.inTerminalMenu && !this.playerController.inSpecialInteractAnimation)
			{
				if (!this.playerController.isCrouching)
				{
					cameraPositionGoal = new Vector3(0, -1.1f, 0.3f);
				}
				else
				{
					cameraPositionGoal = new Vector3(0, -0.5f, 0.3f);
				}
			}

			this.playerController.gameplayCamera.transform.localPosition = Vector3.MoveTowards(this.playerController.gameplayCamera.transform.localPosition, cameraPositionGoal, Time.deltaTime * 2);

			// Adjust position constraint to avoid going through the floor.
			if (this.playerController.isCrouching)
			{
				this.torsoConstraint.weight = Mathf.MoveTowards(this.torsoConstraint.weight, 0.5f, Time.deltaTime * 3);
			}
			else
			{
				this.torsoConstraint.weight = Mathf.MoveTowards(this.torsoConstraint.weight, 1f, Time.deltaTime * 3);
			}

			// Adjust torso rotation for climbing animation.
			if (this.playerController.isClimbingLadder)
			{
				this.dogTorso.localRotation = Quaternion.RotateTowards(this.dogTorso.localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * 360);
			}
			else
			{
				this.dogTorso.localRotation = Quaternion.RotateTowards(this.dogTorso.localRotation, Quaternion.Euler(180, 0, 0), Time.deltaTime * 360);
			}

			////// Kill hack for Ragdoll testing purposes.
			////if (!this.playerController.isPlayerDead)
			////{
			////	if (UnityEngine.InputSystem.Keyboard.current.numpad0Key.wasPressedThisFrame)
			////	{
			////		Debug.Log("Trying to kill player.");
			////		this.playerController.KillPlayer(Vector3.up, true, CauseOfDeath.Unknown, 0);
			////	}

			////	if (UnityEngine.InputSystem.Keyboard.current.numpad1Key.wasPressedThisFrame)
			////	{
			////		Debug.Log("Trying to kill player (1).");
			////		this.playerController.KillPlayer(Vector3.up, true, CauseOfDeath.Unknown, 1);
			////	}

			////	if (UnityEngine.InputSystem.Keyboard.current.numpad2Key.wasPressedThisFrame)
			////	{
			////		Debug.Log("Trying to kill player (springman).");
			////		this.playerController.KillPlayer(Vector3.up, true, CauseOfDeath.Unknown, 2);
			////	}

			////	if (UnityEngine.InputSystem.Keyboard.current.numpad3Key.wasPressedThisFrame)
			////	{
			////		Debug.Log("Trying to kill player (electrocution).");
			////		this.playerController.KillPlayer(Vector3.up, true, CauseOfDeath.Unknown, 3);
			////	}
			////}
		}

		private void LateUpdate()
		{
			if (this.localItemAnchor == null || this.serverItemAnchor == null)
			{
				return;
			}

			// Update the location of the item anchor. This is reset by animation between every Update and LateUpate.
			// Thanks to the DefaultExecutionOrder attribute we know it'll be executed BEFORE the GrabbableObject.LateUpdate().
			if (this.isDogActive)
			{
				this.playerController.localItemHolder.position = this.localItemAnchor.position;
				this.playerController.serverItemHolder.position = this.serverItemAnchor.position;
			}

			// Make sure the shadow casting mode and layer are right despite other mods.
			if (this.dogRenderers[0].shadowCastingMode != this.playerController.thisPlayerModel.shadowCastingMode)
			{
				Debug.Log($"Dog model is on the wrong shadow casting mode. ({this.dogRenderers[0].shadowCastingMode} instead of {this.playerController.thisPlayerModel.shadowCastingMode})");
				this.dogRenderers[0].shadowCastingMode = this.playerController.thisPlayerModel.shadowCastingMode;
			}

			if (this.dogRenderers[0].gameObject.layer != this.playerController.thisPlayerModel.gameObject.layer)
			{
				Debug.Log($"Dog model is on the wrong layer. ({LayerMask.LayerToName(this.dogRenderers[0].gameObject.layer)} instead of {LayerMask.LayerToName(this.playerController.thisPlayerModel.gameObject.layer)})");
				this.dogRenderers[0].gameObject.layer = this.playerController.thisPlayerModel.gameObject.layer;
			}
		}

		private void SpawnDogModel()
		{
			try
			{
				// Load and spawn new model.
				GameObject modelPrefab = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<GameObject>("assets/Dog.fbx");
				this.dogGameObject = Instantiate(modelPrefab, this.transform);
				this.dogGameObject.transform.position = this.transform.position;
				this.dogGameObject.transform.eulerAngles = this.transform.eulerAngles;
				this.dogGameObject.transform.localScale *= 2f;
			}
			catch (System.Exception e)
			{
				PlayerModelReplacer.exceptionMessage = "Failed to spawn dog model.";
				PlayerModelReplacer.exception = e;

				Debug.LogError(PlayerModelReplacer.exceptionMessage);
				Debug.LogException(PlayerModelReplacer.exception);
			}

			// Copy the material. Note: this is also changed in the Update.
			this.dogRenderers = this.dogGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			this.UpdateMaterial();

			try
			{
				// Enable LOD. This is both for performances and being visible in cameras. Values are simply copied from the human LOD.
				LODGroup lodGroup = this.dogGameObject.AddComponent<LODGroup>();
				lodGroup.fadeMode = LODFadeMode.None;
				LOD lod1 = new LOD() { screenRelativeTransitionHeight = 0.4564583f, renderers = new Renderer[] { this.dogRenderers[0] }, fadeTransitionWidth = 0f };
				LOD lod2 = new LOD() { screenRelativeTransitionHeight = 0.1795709f, renderers = new Renderer[] { this.dogRenderers[1] }, fadeTransitionWidth = 0f };
				LOD lod3 = new LOD() { screenRelativeTransitionHeight = 0.009000001f, renderers = new Renderer[] { this.dogRenderers[2] }, fadeTransitionWidth = 0.435f };
				lodGroup.SetLODs(new LOD[] { lod1, lod2, lod3 });
			}
			catch (System.Exception e)
			{
				PlayerModelReplacer.exceptionMessage = "Failed to set up the LOD.";
				PlayerModelReplacer.exception = e;

				Debug.LogError(PlayerModelReplacer.exceptionMessage);
				Debug.LogException(PlayerModelReplacer.exception);
			}

			try
			{
				// Set up the anim correspondence with Constraints.
				this.dogTorso = this.dogGameObject.transform.Find("Armature").Find("torso");
				Transform dogHead = this.dogTorso.Find("head");
				Transform dogArmL = this.dogTorso.Find("arm.L");
				Transform dogArmR = this.dogTorso.Find("arm.R");
				Transform dogLegL = this.dogTorso.Find("butt").Find("leg.L");
				Transform dogLegR = this.dogTorso.Find("butt").Find("leg.R");

				Transform humanPelvis = this.transform.Find("ScavengerModel").Find("metarig").Find("spine");
				Transform humanHead = humanPelvis.Find("spine.001").Find("spine.002").Find("spine.003").Find("spine.004");
				Transform humanLegL = humanPelvis.Find("thigh.L");
				Transform humanLegR = humanPelvis.Find("thigh.R");

				try
				{
					// Add Constraints.
					this.torsoConstraint = this.dogTorso.gameObject.AddComponent<PositionConstraint>();
					this.torsoConstraint.AddSource(new ConstraintSource() { sourceTransform = humanPelvis, weight = 1 });
					this.torsoConstraint.translationAtRest = this.dogTorso.localPosition;
					this.torsoConstraint.translationOffset = this.dogTorso.InverseTransformPoint(humanPelvis.position);
					this.torsoConstraint.constraintActive = true;
					this.torsoConstraint.locked = true;

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
					PlayerModelReplacer.exceptionMessage = "Failed to set up the constraints.";
					PlayerModelReplacer.exception = e;

					Debug.LogError(PlayerModelReplacer.exceptionMessage);
					Debug.LogException(PlayerModelReplacer.exception);
				}

				// Fetch the anchors for the items.
				this.serverItemAnchor = dogHead.Find("serverItem");
				this.localItemAnchor = dogHead.Find("localItem");
			}
			catch (System.Exception e)
			{
				PlayerModelReplacer.exceptionMessage = "Failed to retrieve bones. What the hell?";
				PlayerModelReplacer.exception = e;

				Debug.LogError(PlayerModelReplacer.exceptionMessage);
				Debug.LogException(PlayerModelReplacer.exception);
			}

			// Get a handy list of gameobjects to disable.
			this.humanGameObjects = new GameObject[6];
			this.humanGameObjects[0] = this.playerController.thisPlayerModel.gameObject;
			this.humanGameObjects[1] = this.playerController.thisPlayerModelLOD1.gameObject;
			this.humanGameObjects[2] = this.playerController.thisPlayerModelLOD2.gameObject;
			this.humanGameObjects[3] = this.playerController.thisPlayerModelArms.gameObject;
			this.humanGameObjects[4] = this.playerController.playerBetaBadgeMesh.gameObject;
			this.humanGameObjects[5] = this.playerController.playerBetaBadgeMesh.transform.parent.Find("LevelSticker").gameObject;
		}

		public void EnableHumanModel(bool playAudio = true)
		{
			this.isDogActive = false;

			// Dog can be completely disabled because it doesn't drive the animations and sounds and other stuff.
			this.dogGameObject.SetActive(false);

			// Human renderers have to be directly disabled: the game object contains the camera and stuff and must remain enabled.
			foreach (GameObject humanGameObject in this.humanGameObjects)
			{
				humanGameObject.SetActive(true);
			}

			if (playAudio)
			{
				this.playerController.movementAudio.PlayOneShot(PlayerModelReplacer.humanClip);
			}

			if (this.playerController.IsOwner)
			{
				if (PlayerModelReplacer.healthFill)
				{
					PlayerModelReplacer.healthFill.sprite = PlayerModelReplacer.humanFill;
					PlayerModelReplacer.healthOutline.sprite = PlayerModelReplacer.humanOutline;
				}
			}

			if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
			{
				MoreCompanyPatch.ShowCosmeticsForPlayer(playerController);
			}
		}

		public void EnableDogModel(bool playAudio = true)
		{
			this.isDogActive = true;

			this.dogGameObject.SetActive(true);

			// Make sure the shadow casting mode is the same. Still don't know how the player is visible in cameras but not in first person. It's not a layer thing, maybe it's the LOD?
			this.dogRenderers[0].shadowCastingMode = this.playerController.thisPlayerModel.shadowCastingMode;

			foreach (GameObject humanGameObject in this.humanGameObjects)
			{
				humanGameObject.SetActive(false);
			}

			if (playAudio)
			{
				this.playerController.movementAudio.PlayOneShot(PlayerModelReplacer.dogClip);
			}

			if (this.playerController.IsOwner)
			{
				if (!PlayerModelReplacer.healthFill)
				{
					PlayerModelReplacer.healthFill = HUDManager.Instance.selfRedCanvasGroup.GetComponent<Image>();
					PlayerModelReplacer.healthOutline = HUDManager.Instance.selfRedCanvasGroup.transform.parent.Find("Self").GetComponent<Image>();

					PlayerModelReplacer.humanFill = PlayerModelReplacer.healthFill.sprite;
					PlayerModelReplacer.humanOutline = PlayerModelReplacer.healthOutline.sprite;
				}

				PlayerModelReplacer.healthFill.sprite = PlayerModelReplacer.dogFill;
				PlayerModelReplacer.healthOutline.sprite = PlayerModelReplacer.dogOutline;
			}

			if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
			{
				MoreCompanyPatch.HideCosmeticsForPlayer(playerController);
			}
		}

		public void UpdateMaterial()
		{
			if (this.dogRenderers == null)
			{
				Debug.LogWarning($"Skipping material replacement on dog because there was an error earlier.");
				return;
			}

			foreach (Renderer renderer in this.dogRenderers)
			{
				renderer.material = this.playerController.thisPlayerModel.material;
			}
		}

		public void ToggleAndBroadcast(bool playAudio = true)
		{
			if (this.isDogActive)
			{
				this.EnableHumanModel();
			}
			else
			{
				this.EnableDogModel();
			}

			this.BroadcastSelectedModel(playAudio);
		}

		public void BroadcastSelectedModel(bool playAudio)
		{
			Debug.Log($"Sent dog={this.isDogActive} on {this.playerController.playerClientId} ({this.playerController.playerUsername}).");

			ToggleData data = new ToggleData()
			{
				playerClientId = this.PlayerClientId,
				isDog = this.isDogActive,
				playAudio = playAudio,
			};

			LC_API.Networking.Network.Broadcast(Networking.ModelSwitchMessageName, data);
		}

		public static void RequestSelectedModelBroadcast()
		{
			LC_API.Networking.Network.Broadcast(Networking.ModelInfoMessageName);
		}

		private static void LoadImageResources()
		{
			try
			{
				Texture2D filled = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<Texture2D>("assets/TPoseFilled.png");
				PlayerModelReplacer.dogFill = Sprite.Create(filled, new Rect(0, 0, filled.width, filled.height), new Vector2(0.5f, 0.5f), 100f);

				Texture2D outline = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<Texture2D>("assets/TPoseOutline.png");
				PlayerModelReplacer.dogOutline = Sprite.Create(outline, new Rect(0, 0, outline.width, outline.height), new Vector2(0.5f, 0.5f), 100f);
			}
			catch (System.Exception e)
			{
				PlayerModelReplacer.exceptionMessage = "Failed to retrieve images.";
				PlayerModelReplacer.exception = e;

				Debug.LogError(PlayerModelReplacer.exceptionMessage);
				Debug.LogException(PlayerModelReplacer.exception);
			}
		}

		private static IEnumerator LoadAudioResources()
		{
			string fullPath = GetAssemblyFullPath("ChangeSuitToHuman.wav");
			UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				PlayerModelReplacer.humanClip = DownloadHandlerAudioClip.GetContent(request);
				PlayerModelReplacer.humanClip.name = Path.GetFileName(fullPath);
			}

			fullPath = GetAssemblyFullPath("ChangeSuitToDog.wav");
			request = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				PlayerModelReplacer.dogClip = DownloadHandlerAudioClip.GetContent(request);
				PlayerModelReplacer.dogClip.name = Path.GetFileName(fullPath);
			}
		}

		private static string GetAssemblyFullPath(string additionalPath)
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = ((additionalPath != null) ? Path.Combine(directoryName, ".\\" + additionalPath) : directoryName);
			return Path.GetFullPath(path);
		}

		[JsonObject]
		internal class ToggleData
		{
			[JsonProperty]
			public ulong playerClientId
			{
				get;
				set;
			}

			[JsonProperty]
			public bool isDog
			{
				get;
				set;
			}

			[JsonProperty]
			public bool playAudio
			{
				get;
				set;
			}
		}
	}
}

