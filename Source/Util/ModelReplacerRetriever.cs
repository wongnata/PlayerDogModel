using PlayerDogModel_Plus.Source.Model;
using System.Collections.Generic;

namespace PlayerDogModel_Plus.Source.Util
{
    internal sealed class ModelReplacerRetriever
    {
        private static Dictionary<ulong, PlayerModelReplacer> cache = new Dictionary<ulong, PlayerModelReplacer>();

        internal static PlayerModelReplacer GetModelReplacerFromClientId(ulong clientId)
        {
            if (cache.TryGetValue(clientId, out PlayerModelReplacer cachedReplacer)) return cachedReplacer;

            if (PlayerRetriever.GetPlayerFromClientId(clientId).TryGetComponent<PlayerModelReplacer>(out PlayerModelReplacer replacer))
            {
                cache.Add(clientId, replacer);
                return replacer;
            }

            Plugin.logger.LogDebug($"Couldn't find replacer with for player with clientId={clientId}!");
            return null;
        }

        internal static void OnClientDisconnect(ulong clientId)
        {
            cache.Remove(clientId);
        }
    }
}
