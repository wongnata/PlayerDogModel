using GameNetcodeStuff;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.Animations;

namespace PlayerDogModel_Plus
{
	public class PlayerModelSwitcher : MonoBehaviour
	{
		private void Start()
		{
			// Spawn the mesh.
			GameObject modelPrefab = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<GameObject>("assets/Helmets.fbx");
			GameObject modelInstance = Instantiate(modelPrefab, this.transform);
			modelInstance.transform.localPosition = Vector3.zero;
			modelInstance.transform.localRotation = Quaternion.identity;
			modelInstance.transform.localScale = Vector3.one * 0.75f;

			// Find and reference the materials.
			Renderer renderer = modelInstance.GetComponent<Renderer>();
			Material[] materials = renderer.materials;
			for (int i = 0; i < StartOfRound.Instance.unlockablesList.unlockables.Count; i++)
			{
				if (StartOfRound.Instance.unlockablesList.unlockables[0].suitMaterial)
				{
					materials[0] = StartOfRound.Instance.unlockablesList.unlockables[0].suitMaterial;
					break;
				}
			}

			materials[1] = this.GetComponent<Renderer>().material;
			renderer.materials = materials;

			// Add the scan node (copy an existing one because it's easier and it works).
			GameObject scanNode = GameObject.Instantiate(GameObject.FindObjectOfType<ScanNodeProperties>()).gameObject;
			scanNode.transform.parent = this.transform;
			scanNode.transform.localPosition = new Vector3(0.75f, 0, 0.8f);

			ScanNodeProperties scanNodeProperties = scanNode.GetComponent<ScanNodeProperties>();
			scanNodeProperties.headerText = "Dog equipment";
			scanNodeProperties.subText = "Switch to the dog model here.";

			// Add the interaction.
			InteractTrigger triggerPrefab = GameObject.Find("SpeakerAudio").transform.parent.GetComponentInChildren<InteractTrigger>();
			InteractTrigger interactionTrigger = GameObject.Instantiate(triggerPrefab);
			interactionTrigger.transform.position = this.transform.TransformPoint(new Vector3(0.75f, 0, 0.9f));
			interactionTrigger.transform.localScale = new Vector3(0.3f, 0.7f, 0.3f);

			interactionTrigger.hoverTip = "Switch model";
			interactionTrigger.interactable = true;
			interactionTrigger.cooldownTime = 1;
			interactionTrigger.onCancelAnimation.RemoveAllListeners();
			interactionTrigger.onInteractEarly.RemoveAllListeners();
			interactionTrigger.onStopInteract.RemoveAllListeners();
			interactionTrigger.onInteract.RemoveAllListeners();
			interactionTrigger.onInteract.AddListener(this.Interacted);
		}

		private void Interacted(PlayerControllerB player)
		{
			player.GetComponentInChildren<PlayerModelReplacer>().ToggleAndBroadcast();
		}
	}
}