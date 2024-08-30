using BepInEx;
using HarmonyLib;
using System.Reflection;
using GameNetcodeStuff;
using UnityEngine;
using System.IO;
using UnityEngine.Animations;

namespace PlayerDogModel
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("LC_API_V50")]
	[BepInProcess("Lethal Company.exe")]
	public class Plugin : BaseUnityPlugin
	{
		public static Harmony _harmony;

		private void Awake()
		{
			_harmony = new Harmony(PluginInfo.PLUGIN_GUID);
			_harmony.PatchAll();
			Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");

			Networking.Initialize();
			LC_API.BundleAPI.BundleLoader.LoadAssetBundle(GetAssemblyFullPath("playerdog"));
		}

		private static string GetAssemblyFullPath(string additionalPath)
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = ((additionalPath != null) ? Path.Combine(directoryName, ".\\" + additionalPath) : directoryName);
			return Path.GetFullPath(path);
		}

		// PlayerModelReplacer handles the model and its toggling.
		[HarmonyPatch(typeof(PlayerControllerB))]
		internal class PlayerControllerBPatch
		{
			// SpawnPlayerAnimation is called when respawning.
			[HarmonyPatch("SpawnPlayerAnimation")]
			[HarmonyPostfix]
			public static void SpawnPlayerAnimationPatch(ref PlayerControllerB __instance)
			{
				// Find all the players and add the script to them if they don't have it yet.
				// This is done for every player every time a player spawns just to be sure.
				foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
				{
					if (!player.GetComponent<PlayerModelReplacer>())
					{
						player.gameObject.AddComponent<PlayerModelReplacer>();
					}
				}

				// Request data regarding the other players' skins.
				PlayerModelReplacer.RequestSelectedModelBroadcast();
			}
		}

		// PlayerModelSwitcher is the interaction which allows the player to toggle the model on and off.
		[HarmonyPatch(typeof(StartOfRound))]
		internal class StartOfRoundPatch
		{
			// PositionSuitsOnRack is called when the game scene is prepared.
			[HarmonyPatch("PositionSuitsOnRack")]
			[HarmonyPostfix]
			public static void PositionSuitsOnRackPatch(ref StartOfRound __instance)
			{
				PlayerModelSwitcher switcher = GameObject.FindObjectOfType<PlayerModelSwitcher>();
				if (!switcher)
				{
					// This weird name is the suits hanger.
					GameObject suitHanger = GameObject.Find("NurbsPath.002");
					suitHanger.AddComponent<PlayerModelSwitcher>();
				}
			}
		}

		// This allows to wear other suits.
		[HarmonyPatch(typeof(UnlockableSuit))]
		internal class UnlockableSuitPatch
		{
			// SwitchSuitForPlayer is called when switching to a new suit.
			[HarmonyPatch("SwitchSuitForPlayer")]
			[HarmonyPostfix]
			public static void SwitchSuitForPlayerPatch(PlayerControllerB player, int suitID, bool playAudio = true)
			{
				PlayerModelReplacer replacer = player.GetComponent<PlayerModelReplacer>();
				if (replacer)
				{
					replacer.UpdateMaterial();
				}
			}
		}

		// Ragdoll support! It's whacky who cares.
		[HarmonyPatch(typeof(DeadBodyInfo))]
		internal class DeadBodyPatch
		{
			// Start is called when the ragdoll is instantiated.
			[HarmonyPatch("Start")]
			[HarmonyPostfix]
			public static void StartPatch(ref DeadBodyInfo __instance)
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
				GameObject dogGameObject = Instantiate(modelPrefab, __instance.transform);
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
				RotationConstraint headConstraint = dogHead.gameObject.AddComponent<RotationConstraint>();
				headConstraint.AddSource(new ConstraintSource() { sourceTransform = humanHead, weight = 1 });
				headConstraint.rotationAtRest = dogHead.localEulerAngles;
				headConstraint.constraintActive = true;
				headConstraint.locked = true;

				RotationConstraint armLConstraint = dogArmL.gameObject.AddComponent<RotationConstraint>();
				armLConstraint.AddSource(new ConstraintSource() { sourceTransform = humanArmR, weight = 1 });
				armLConstraint.rotationAtRest = dogArmL.localEulerAngles;
				armLConstraint.constraintActive = true;
				armLConstraint.locked = true;

				RotationConstraint armRConstraint = dogArmR.gameObject.AddComponent<RotationConstraint>();
				armRConstraint.AddSource(new ConstraintSource() { sourceTransform = humanArmL, weight = 1 });
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
		}
	}
}