using PlayerDogModel_Plus.Source.Model;
using System;
using UnityEngine;

namespace PlayerDogModel_Plus.Source
{
    static class Networking
    {
        public const string ModelSwitchMessageName = "modelswitch";
        public const string ModelInfoMessageName = "modelinfo";

        public static void Initialize()
        {
            LC_API.Networking.Network.RegisterMessage<PlayerModelReplacer.ToggleData>(ModelSwitchMessageName, relayToSelf: false, onReceived: HandleModelSwitchMessage);
            LC_API.Networking.Network.RegisterMessage(ModelInfoMessageName, relayToSelf: false, onReceived: HandleModelInfoMessage);
        }

        private static void HandleModelSwitchMessage(ulong senderId, PlayerModelReplacer.ToggleData toggleData)
        {
            Plugin.logger.LogDebug($"Got {ModelSwitchMessageName} network message from {senderId}: {{ " +
                  $"{nameof(PlayerModelReplacer.ToggleData.playerClientId)} = {toggleData.playerClientId}, " +
                  $"{nameof(PlayerModelReplacer.ToggleData.playAudio)} = {toggleData.playAudio}, " +
                  $"{nameof(PlayerModelReplacer.ToggleData.isDog)} = {toggleData.isDog} " +
                  "}}");

            PlayerModelReplacer replacer = null;

            foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
            {
                var currentReplacer = player.GetComponent<PlayerModelReplacer>();
                if (currentReplacer != null && currentReplacer.PlayerClientId == toggleData.playerClientId)
                {
                    replacer = currentReplacer;
                    break;
                }
            }
            if (replacer == null)
            {
                Plugin.logger.LogWarning($"{ModelSwitchMessageName} message from client {senderId} will be ignored because replacer with this ID is not registered");
                return;
            }

            if (!replacer.IsValid)
            {
                Plugin.logger.LogError("Dog encountered an error when it was initialized and it can't be toggled. Check the log for more info.");
                return;
            }

            Plugin.logger.LogDebug($"Received dog={toggleData.isDog} for {replacer.PlayerClientId} ({replacer.PlayerUsername}).");
            replacer.ReceiveBroadcastAndToggle(toggleData.playAudio, toggleData.isDog);
        }

        private static void HandleModelInfoMessage(ulong senderId)
        {
            Plugin.logger.LogDebug($"Got {ModelInfoMessageName} network message from {senderId}");

            foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
            {
                var replacer = player.GetComponent<PlayerModelReplacer>();
                if (replacer != null)
                {
                    try
                    {
                        replacer.BroadcastSelectedModel(playAudio: false);
                    }
                    catch (Exception e)
                    {
                        Plugin.logger.LogDebug($"Couldn't broadcast model for senderId={senderId} for some reason!");

                        if (!Plugin.boundConfig.suppressExceptions.Value) throw e;
                    }
                }
            }
        }
    }
}