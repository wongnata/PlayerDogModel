using PlayerDogModel_Plus.Source.Model;

namespace PlayerDogModel_Plus.Source.Util
{
    internal sealed class ModelReplacerRetriever
    {
        internal static PlayerModelReplacer GetModelReplacerFromClientId(ulong clientId)
        {
            if (PlayerRetriever.GetPlayerFromClientId(clientId).TryGetComponent<PlayerModelReplacer>(out PlayerModelReplacer replacer))
            {
                return replacer;
            }

            Plugin.logger.LogDebug($"Couldn't find replacer with for player with clientId={clientId}!");
            return null;
        }
    }
}
