using GameNetcodeStuff;

namespace PlayerDogModel_Plus.Source.Util
{
    internal class PlayerRetriever
    {
        internal static PlayerControllerB GetPlayerFromClientId(ulong clientId)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId == clientId)
                {
                    return player;
                }
            }

            Plugin.logger.LogWarning($"Couldn't find player with clientId={clientId}!");
            return null;
        }
    }
}
