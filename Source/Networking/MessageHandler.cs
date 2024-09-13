using GameNetcodeStuff;
using LethalNetworkAPI;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Patches.Core;
using PlayerDogModel_Plus.Source.Util;
using System;
using Unity.Netcode;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Networking
{
    static class MessageHandler
    {
        public const string ModelSwitchMessageName = "modelswitch";
        public const string ModelInfoMessageName = "modelinfo";
        public const string MaskedDogSpawnMessageName = "maskeddogspawn";

        public static void Initialize()
        {
            LNetworkMessage<string> selectedModelMessage = LNetworkMessage<string>.Connect(ModelSwitchMessageName);
            selectedModelMessage.OnClientReceivedFromClient += HandleModelSwitchMessage;

            LNetworkMessage<string> maskedDogSpawnedEvent = LNetworkMessage<string>.Connect(MaskedDogSpawnMessageName);
            maskedDogSpawnedEvent.OnClientReceivedFromClient += HandleMaskedDogSpawnMessage;

            // Using message here since for some reason event isn't working for me yet
            LNetworkMessage<string> requestSelectedModelEvent = LNetworkMessage<string>.Connect(ModelInfoMessageName);
            requestSelectedModelEvent.OnClientReceivedFromClient += HandleModelInfoMessage;
        }

        internal static void HandleModelSwitchMessage(string modelToggleJson, ulong senderId)
        {
            ModelToggleData modelToggleData = JsonUtility.FromJson<ModelToggleData>(modelToggleJson);
            Plugin.logger.LogDebug($"Got {ModelSwitchMessageName} network message from {senderId} with json={modelToggleJson}");

            PlayerControllerB player = PlayerRetriever.GetPlayerFromClientId(modelToggleData.clientId);
            PlayerModelReplacer replacer = player.GetComponent<PlayerModelReplacer>();

            if (replacer == null)
            {
                Plugin.logger.LogWarning($"{ModelSwitchMessageName} message from client {modelToggleData.clientId} will be ignored because replacer with this ID is not registered");
                return;
            }

            if (!replacer.IsValid)
            {
                Plugin.logger.LogError("Dog encountered an error when it was initialized and it can't be toggled. Check the log for more info.");
                return;
            }

            replacer.ReceiveBroadcastAndToggle(false, modelToggleData.isDog);
        }

        internal static void HandleModelInfoMessage(string nothing, ulong senderId) // Literally not using the string param
        {
            Plugin.logger.LogDebug($"Got {ModelInfoMessageName} network message from {senderId}");
            PlayerControllerB localPlayer = StartOfRound.Instance.localPlayerController;
            PlayerModelReplacer replacer = localPlayer.GetComponent<PlayerModelReplacer>();

            if (replacer != null)
            {
                try
                {
                    replacer.BroadcastSelectedModel(playAudio: false);
                }
                catch (Exception e)
                {
                    Plugin.logger.LogDebug($"Couldn't broadcast model for senderId={senderId} for some reason!");

                    if (!Plugin.config.suppressExceptions.Value) throw e;
                }
            }
        }

        internal static void HandleMaskedDogSpawnMessage(string maskedDogJson, ulong senderId)
        {
            Plugin.logger.LogDebug($"Got {MaskedDogSpawnMessageName} network message from {senderId} with json={maskedDogJson}");
            MaskedDogData maskedDogData = JsonUtility.FromJson<MaskedDogData>(maskedDogJson);

            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(maskedDogData.maskedEnemyNetworkId, out NetworkObject networkObject))
            {
                Plugin.logger.LogDebug($"Couldn't find networkObjectId={maskedDogData.maskedEnemyNetworkId}");
                return;
            }

            MaskedPlayerEnemy mimic = networkObject.GetComponent<MaskedPlayerEnemy>();
            if (mimic == null)
            {
                Plugin.logger.LogDebug($"Couldn't find a MaskedPlayerEnemy for networkObjectId={maskedDogData.maskedEnemyNetworkId}");
                return;
            }

            if (mimic.mimickingPlayer == null || mimic.mimickingPlayer.GetClientId() != maskedDogData.mimickingClientId)
            {
                mimic.mimickingPlayer = PlayerRetriever.GetPlayerFromClientId(maskedDogData.mimickingClientId); // Make sure this is consistent
            }

            if (MaskedPlayerEnemyPatch.RenderMaskedDog(ref mimic))
            {
                Plugin.logger.LogDebug($"Rendered a dog model for the mimic with networkObjectId={maskedDogData.maskedEnemyNetworkId}");
                return;
            }
            Plugin.logger.LogDebug($"Didn't render a dog model for the mimic with networkObjectId={maskedDogData.maskedEnemyNetworkId}");
        }
    }
}