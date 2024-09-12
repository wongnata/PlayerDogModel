using GameNetcodeStuff;
using System.Collections.Generic;

namespace PlayerDogModel_Plus.Source.Util
{
    internal class PlayerRetriever
    {
        private static Dictionary<ulong, PlayerControllerB> cache = new Dictionary<ulong, PlayerControllerB>();

        internal static PlayerControllerB GetPlayerFromClientId(ulong clientId)
        {
            // This works most of the time for a direct lookup
            if (StartOfRound.Instance.ClientPlayerList.TryGetValue(clientId, out int playerScriptIndex))
            {
                return StartOfRound.Instance.allPlayerScripts[playerScriptIndex];
            }

            if (cache.TryGetValue(clientId, out PlayerControllerB cachedPlayer)) return cachedPlayer;

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId == clientId)
                {
                    cache.Add(clientId, player);
                    return player;
                }
            }

            Plugin.logger.LogWarning($"Couldn't find player with clientId={clientId}!");
            return null;
        }

        internal static void OnClientDisconnect(ulong clientId)
        {
            cache.Remove(clientId);
        }
    }
}
