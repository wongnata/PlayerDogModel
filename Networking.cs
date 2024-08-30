using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace PlayerDogModel_Plus
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
            Debug.Log($"Got {ModelSwitchMessageName} network message from {senderId}: {{ " +
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
                Debug.LogWarning($"{ModelSwitchMessageName} message from client {senderId} will be ignored because replacer with this ID is not registered");
                return;
            }

            if (!replacer.IsValid)
            {
                Debug.LogError("Dog encountered an error when it was initialized and it can't be toggled. Check the log for more info.");
                return;
            }

            Debug.Log($"Received dog={toggleData.isDog} for {replacer.PlayerClientId} ({replacer.PlayerUsername}).");

            if (toggleData.isDog)
            {
                replacer.EnableDogModel(toggleData.playAudio);
            }
            else
            {
                replacer.EnableHumanModel(toggleData.playAudio);
            }
        }

        private static void HandleModelInfoMessage(ulong senderId)
        {
            Debug.Log($"Got {ModelInfoMessageName} network message from {senderId}");
            foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
            {
                var replacer = player.GetComponent<PlayerModelReplacer>();
                if (replacer != null)
                {
                    replacer.BroadcastSelectedModel(playAudio: false);
                }
            }
        }
    }
}