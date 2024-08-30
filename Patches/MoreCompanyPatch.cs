using GameNetcodeStuff;
using MoreCompany;
using MoreCompany.Cosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerDogModel_Plus.Patches
{

	public static class MoreCompanyPatch
	{
		public static void HideCosmeticsForPlayer(PlayerControllerB playerController)
		{
			CosmeticApplication cosmeticApplication = playerController.meshContainer.GetComponentInChildren<CosmeticApplication>();

			if (cosmeticApplication == null)
			{
				Debug.Log($"{PluginInfo.PLUGIN_GUID}: {playerController.playerUsername}'s cosmetic application's instance was null!");
				return;
			}

            Debug.Log($"{PluginInfo.PLUGIN_GUID}: {playerController.playerUsername}'s cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");

            cosmeticApplication.ClearCosmetics();

			List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
			foreach (var selected in selectedCosmetics)
			{
				Debug.Log($"{PluginInfo.PLUGIN_GUID}: Disabling {playerController.playerUsername}'s {selected}...");
				cosmeticApplication.ApplyCosmetic(selected, false);
			}

			cosmeticApplication.RefreshAllCosmeticPositions();
			foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
			{
				cosmetic.transform.localScale *= CosmeticRegistry.COSMETIC_PLAYER_SCALE_MULT;
			}
		}

		public static void ShowCosmeticsForPlayer(PlayerControllerB playerController)
		{
			CosmeticApplication cosmeticApplication = playerController.meshContainer.GetComponentInChildren<CosmeticApplication>();

			if (cosmeticApplication == null)
			{
				Debug.Log($"{PluginInfo.PLUGIN_GUID}: {playerController.playerUsername}'s cosmetic application's instance was null!");
				return;
			}

            Debug.Log($"{PluginInfo.PLUGIN_GUID}: {playerController.playerUsername}'s cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");

            cosmeticApplication.ClearCosmetics();
			List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
			foreach (var selected in selectedCosmetics)
			{
				Debug.Log($"{PluginInfo.PLUGIN_GUID}: Enabling {playerController.playerUsername}'s {selected}...");
				cosmeticApplication.ApplyCosmetic(selected, true);
			}

			cosmeticApplication.RefreshAllCosmeticPositions();
			foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
			{
				cosmetic.transform.localScale *= CosmeticRegistry.COSMETIC_PLAYER_SCALE_MULT;

			}
		}
	}
}
