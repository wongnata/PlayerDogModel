using GameNetcodeStuff;
using LethalNetworkAPI;
using PlayerDogModel_Plus.Source.Model;
using PlayerDogModel_Plus.Source.Util;
using System;
using UnityEngine;

namespace PlayerDogModel_Plus.Source.Networking
{
    static class MessageHandler
    {
        public const string ModelSwitchMessageName = "modelswitch";
        public const string ModelInfoMessageName = "modelinfo";

        public static void Initialize()
        {
            LNetworkMessage<string> selectedModelMessage = LNetworkMessage<string>.Connect(ModelSwitchMessageName);
            LNetworkEvent requestSelectedModelEvent = LNetworkEvent.Connect(ModelInfoMessageName);
            selectedModelMessage.OnClientReceivedFromClient += HandleModelSwitchMessage;
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

        internal static void HandleModelInfoMessage(ulong senderId)
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
    }
}