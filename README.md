

# [Fork of the original mod by **MonAmiral**!](https://thunderstore.io/c/lethal-company/p/MonAmiral/PlayerDogModel/)

![](https://i.imgur.com/s1SdJxD.png)

<details>

<summary>Click for more screenshots!</summary>

![](https://imgur.com/HqYB9te.png)
![](https://i.imgur.com/lJHsS3n.png)
![](https://i.imgur.com/dSnw0l3.png)
![](https://i.imgur.com/NS6bAPH.png)

</details>

## Features
- Use the helmets on the side of the suits rack to switch between dog and human model!
- Replaces your 3D model, adjusts your height, updates the health UI.
- Compatible with custom suits!
- Compatible with 3rd person & mirror!
- The mod works in multiplayer with players who do not have it! They will just see everyone as a human.
- Limited compatibility with emotes!
- **[NEW]** Compatible with MoreCompany cosmetics! Cosmetics are hidden in dog mode.
- **[NEW]** Configurable with Verity-3rdPerson! Overrides are available using the config file.

## Limitations
- Not compatible with model replacing mods (e.g. ModelReplacementAPI, etc.)
- When crouching in dog mode, dropped items can fall through the ship
- Masked enemies spawned from dog model players will still use their human model
- When using the jetpack in dog mode, the jetpack model partially obstructs your view

## Credits
- [Obviously **MonAmiral** for creating the original mod! It's awesome!](https://thunderstore.io/c/lethal-company/p/MonAmiral/PlayerDogModel/)
- Based on [DarnHyena's LethalCreatures mod, which is based on code by Zoomy](https://github.com/DarnHyena/LethalCreatures)
- Originated from a [design by EndlessForebode](https://twitter.com/UslurpArt/status/1724137874717573268)
- [LC_API 3.2 fix by juanjp600. Thank you very much!](https://github.com/MonAmiral/PlayerDogModel/pull/12)
- Thanks to Andrew, Jaime, Andy, and Denny for your help in testing and fixing multiplayer interactions! Dog bless you!
- Thanks to the entire Flodogs+ squad for dogfooding this mod with me! Bone-appetit!

## Changelog

**[1.0.0]**
- Updated to LC_API_V50. 
- Added compatibility with MoreCompany which hides cosmetics when in dog mode.

**[1.0.1]**
- Fixed issue causing your own MoreCompany cosmetics to be visible in first-person. 
- Added dependency to x753-More_Suits since this mod tends to work best with it.

**[1.0.2]**
- Dog model ragdolls no longer have MoreCompany cosmetics floating above them. 
- Fixed audio issue where model toggle clips are played on respawn.

**[1.1.0]**
- Updated dog model ragdolls so that back legs and torso also move.
- Updated spectator camera to be better centered around players in dog mode.
- Updated snare flea interaction so that snare fleas attach to the dog model's head.
- Updated all logging to use BepIn Ex loggers.
- Added a config file for Verity-3rdPerson overrides.
- Added patch for Verity-3rdPerson which will use custom overrides for dog mode.
- Raised camera height of dog mode to better match the dog model.
- Updated the item anchors for dog mode to prevent dropped items from clipping through the ship.
- Added dependencies to Verity-3rdPerson and MoreCompany for now (as a workaround for the plugin breaking without them)