using GameNetcodeStuff;
using PlayerDogModel_Plus.Source.Model;
using TerminalApi.Classes;

namespace PlayerDogModel_Plus.Source.Terminal
{
    internal class SwitchModelCommand
    {
        internal static void Initialize()
        {
            TerminalApi.TerminalApi.AddCommand("toggledog", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
                    PlayerModelReplacer replacer = localPlayer.GetComponent<PlayerModelReplacer>();
                    bool wasDog = replacer.IsDog;

                    PlayerModelSwitcher.SwitchModel(localPlayer);
                    string currentModel = wasDog ? "human" : "dog";
                    return "Model switched!\n\n" + $"You're now a {currentModel}!\n\n";
                },
                Category = "Other",
                Description = "Toggle the dog model"
            });
        }
    }
}
