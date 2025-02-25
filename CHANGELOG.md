# Changelog

**[2.0.10]**
- Rebuilt and updated OpenBodyCams compatibility to use new API introduced in OpenBodyCams v3.0.1

**[2.0.9]**
- Rebuilt for compatibility with MoreCompany v1.11.0

**[2.0.8]**
- Removed broken patch for OpenBodyCams which attempts to hide MoreCompany cosmetics. Disable EnableMoreCompanyCosmeticsCompatibility in the OpenBodyCams config to hide MoreCompany cosmetics in the camera instead!

**[2.0.7]**
- Fixed issue where TooManyEmotes showed floating MoreCompany cosmetics in third person when in dog mode
- Fixed issue causing OpenBodyCams to show your own MoreCompany cosmetics in some third person cameras
- Added a soft dependency on TerminalAPI allowing dog mode to be toggled through the terminal (for compatibility with SuitsTerminal, etc.)
- Updated the way optional patches are loaded to be less brittle if patching fails

**[2.0.6]**
- Added support for OpenBodyCams! When viewing dog model players on the monitor, their camera perspectives will no longer be obstructed by the dog model.

**[2.0.5]**
- Added MIT license for distribution and credit! Thanks again to MonAmiral for his blessing! <3

**[2.0.4]**
- Fixed tulip snake interaction so that tulip snakes attach to dog players' heads
- Fixed vanilla suits (bunny, bee) with head cosmetics so they no longer show floating head cosmetics on dog players
- Updated local player lookup to be more consistent with how lookup is done in the game logic (using GameNetworkManager and not StartOfRound)

**[2.0.3]**
- Forgot to update the version of the DLL last release. Updating now to avoid any dependency issues.

**[2.0.2]**
- Attempted a fix for dog model desyncs for clients especially during disconnects
- Attempted another fix for dog player reconnects spawning as dogs for some clients

**[2.0.1]**
- Updated networking from LethalNetworkAPI v2 to v3
- Minor performance improvements (made most debug logging debug-build only)
- Fixed desync issue with masked enemies using dog models

**[2.0.0]**
- Added support for masked enemies! Masked enemies that spawn from dog players will now use the dog model and no masks
- Added a new config file setting to disable masks on dog mimics

**[1.1.4]**
- Removed patch for adding blood decals to a dog player
- Updated masks held position and possession position to align to dog model
- Updated belt bag pocketed position to be accessible by dog players (and look more like a collar!)
- Updated jetpack position to no longer obstruct vision in dog mode (and now the alignment makes some sense physics-wise!)

**[1.1.3]**
- Removed dependency on LC_API_V50 and More_Suits
- Added dependency on LethalNetworkAPI and reimplemented networking
- Fixed issue where after a dog player disconnects and rejoins, they will be in dog mode (with visible floating MoreCompany cosmetics if applicable)
- Updated belt bag alignment to better fit the dog model

**[1.1.2]**
- Updated dependency on LC_API_V50 to use the latest version for v64 support.

**[1.1.1]**
- Fixed issue causing dog mode players appearing to "swim" through the ground while crouching
- Removed hard dependencies to Verity-3rdPerson and MoreCompany
- "Player collected" notification now displays a dog model turnaround when collecting dog player bodies

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
- Updated jetpack alignment to better fit on dog model

**[1.0.2]**
- Dog model ragdolls no longer have MoreCompany cosmetics floating above them. 
- Fixed audio issue where model toggle clips are played on respawn.

**[1.0.1]**
- Fixed issue causing your own MoreCompany cosmetics to be visible in first-person. 
- Added dependency to x753-More_Suits since this mod tends to work best with it.

**[1.0.0]**
- Updated to LC_API_V50. 
- Added compatibility with MoreCompany which hides cosmetics when in dog mode.