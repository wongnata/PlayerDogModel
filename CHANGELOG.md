## Changelog

**[1.1.3]**
- Removed dependency on LC_API_V50 and More_Suits
- Added dependency on LethalNetworkAPI and reimplemented networking
- Fixed issue where after a dog player disconnects and rejoins, they will be in dog mode (with visible floating MoreCompany cosmetics if applicable)

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

**[1.0.1]**
- Fixed issue causing your own MoreCompany cosmetics to be visible in first-person. 
- Added dependency to x753-More_Suits since this mod tends to work best with it.

**[1.0.2]**
- Dog model ragdolls no longer have MoreCompany cosmetics floating above them. 
- Fixed audio issue where model toggle clips are played on respawn.

**[1.0.0]**
- Updated to LC_API_V50. 
- Added compatibility with MoreCompany which hides cosmetics when in dog mode.