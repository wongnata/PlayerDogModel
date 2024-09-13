using System;

namespace PlayerDogModel_Plus.Source.Networking
{
    [Serializable]
    internal class MaskedDogData
    {
        public ulong maskedEnemyNetworkId;
        public ulong mimickingClientId;
    }
}
