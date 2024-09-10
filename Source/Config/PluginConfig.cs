using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace PlayerDogModel_Plus.Source.Config
{
    public class PluginConfig
    {
        // Developer Tools
        public readonly ConfigEntry<bool> suppressExceptions;

        // Verity-3rdPerson Config Overrides
        public readonly ConfigEntry<bool> thirdPersonConfigOverride;
        public readonly ConfigEntry<float> thirdPersonDistance;
        public readonly ConfigEntry<float> thirdPersonRightOffset;
        public readonly ConfigEntry<float> thirdPersonUpOffset;

        // Masked Enemies
        public readonly ConfigEntry<bool> alwaysHideMasksOnDogs;

        public PluginConfig(ConfigFile config)
        {
            config.SaveOnConfigSet = false;

            suppressExceptions = config.Bind(
                "Developer Tools",
                "SuppressExceptions",
                true,
                "Non-fatal exceptions will be suppressed."
            );

            thirdPersonConfigOverride = config.Bind(
                "Verity-3rdPerson Config Overrides",
                "Override",
                true,
                "Enables overriding Verity-3rdPerson config settings with custom ones (distance, right-offset, and up-offset) for dog mode."
            );

            thirdPersonDistance = config.Bind(
                "Verity-3rdPerson Config Overrides",
                "Distance",
                3f,
                "Distance of the camera from the player when using dog mode."
            );

            thirdPersonRightOffset = config.Bind(
                "Verity-3rdPerson Config Overrides",
                "Right-Offset",
                0.0f,
                "Offset of the camera to the right from the player when using dog mode."
            );

            thirdPersonUpOffset = config.Bind(
                "Verity-3rdPerson Config Overrides",
                "Up-Offset",
                0.1f,
                "Offset of the camera upwards from the player when using dog mode."
            );

            alwaysHideMasksOnDogs = config.Bind(
                "Masked Enemies",
                "AlwaysHideMasksOnDogs",
                false,
                "Disables masks on all masked enemies spawned from dogs. Default has masks visible for masked enemies spawned by possession."
            );

            ClearOrphanedEntries(config);
            config.Save();
            config.SaveOnConfigSet = true;
        }


        // Literally pasted from https://lethal.wiki/dev/intermediate/custom-configs, thank you!
        static void ClearOrphanedEntries(ConfigFile config)
        {
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(config);
            orphanedEntries.Clear();
        }
    }
}
