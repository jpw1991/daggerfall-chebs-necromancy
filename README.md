# Cheb's Necromancy

A mod for Daggerfall Unity that adds Necromancy to the game as spells under the Mysticism school.

The undead are clones of vanilla enemies but are allied to the player. They also have code attached to them which makes them follow the player, but the pathfinding isn't great. It's based off how the enemies pursue the player and works basically the same way.

[![video](https://img.youtube.com/vi/pf2PREe3q1E/0.jpg)](https://www.youtube.com/watch?v=pf2PREe3q1E)

##  About Me

[![image1](https://imgur.com/Fahi6sP.png)](https://necrobase.chebgonaz.com)
[![image2](https://imgur.com/X18OyQs.png)](https://www.patreon.com/chebgonaz?fan_landing=true)
[![image3](https://imgur.com/4e64jQ8.png)](https://ko-fi.com/chebgonaz)

I'm a YouTuber/Game Developer/Modder who is interested in all things necromancy and minion-related. Please check out my [YouTube channel](https://www.youtube.com/channel/UCPlZ1XnekiJxKymXbXyvkCg) and if you like the work I do and want to give back, please consider supporting me on [Patreon](https://www.patreon.com/chebgonaz?fan_landing=true) or throwing me a dime on [Ko-fi](https://ko-fi.com/chebgonaz). You can also check out my [website](https://necrobase.chebgonaz.com) where I host information on all known necromancy mods, games, books, videos and also some written reviews/guides.

Thank you and I hope you enjoy the mod! If you have questions or need help please join my [Discord](https://discord.com/invite/EB96ASQ).

## Description

Adds several different spell effects for summoning a variety of undead minions as well as a utility spell to recall these to your position:

- Recall Minions
- Summon Skeleton
- Summon Ghost
- Summon Zombie
- Summon Mummy
- Summon Vampire
- Summon Lich
- Summon Ancient Vampire
- Summon Ancient Lich

Spells can be created at the spell maker in the Mage's Guild.

![image](https://github.com/jpw1991/daggerfall-chebs-necromancy/assets/13718599/d6377ecd-f057-4e6d-8dca-a7f74160ba02)

## Configuration

The costs of the spells can be manipulated via the mod settings. I don't know what all the values do and they're wildly out of whack as of 0.0.5. Help in this department would be immensely appreciated!

![image](https://github.com/jpw1991/daggerfall-chebs-necromancy/assets/13718599/1822a5ee-8ed0-40b4-bb42-392a7c36bad1)

## Debugging

Check the log file and look for "Cheb" to find errors related to this mod.

The log file's [location differs per operating system](https://docs.unity3d.com/Manual/LogFiles.html). On Linux it is located in `$HOME/.config/unity3d/Daggerfall Workshop/Daggerfall Unity/Player.log`

## Cheating

<details>
<summary>Expand to see cheating info (hidden so people's experience doesn't get ruined)</summary>

The spell effects have a backend which can be triggered by console commands:

- Press `~` to open the console
- Type `help spawn` to read the options, then spawn a minion in
  - `spawn skeleton` will spawn a skeletal warrior
  - `spawn vampire` will spawn a vampire, etc.
- Type `recallminions` to bring the undead to your position if they get stuck or lost

</details>

## Installation

- Copy mod to `/path/to/dfu/DaggerfallUnity_Data/StreamingAssets/Mods`
- For more info, please read [here](https://www.dfworkshop.net/projects/daggerfall-unity/modding/#installation)

## Changelog

Version | Date       | Info
--- |------------| ---
0.0.10 | 16/04/2024 | Add basic Animate Dead spell for purchase
0.0.9 | 16/04/2024 | Fix problem where all friendly NPCs get teleported by recall minions.
0.0.8 | 15/04/2024 | Mod should now properly work with built versions of the game
0.0.6 | 15/04/2024 | Fix spell prices not updating properly
0.0.5 | 15/04/2024 | Add spells to mysticism school; Add customizable spell costs
0.0.4 | 14/04/2024 | Remove spawnskeleton command and replace with spawn command that can spawn many different types of undead.
0.0.3 | 14/04/2024 | Restore UndeadMinion script when when game loads so that existing minions resume following; Add command to recall minions to your position; remove skeleton bark
0.0.2 | 05/02/2023 | Add command to spawn skeletons
0.0.1 | 05/02/2023 | Create mod
