# Developer Readme

Want to work on this mod? This may help. Assumes that you have both [git](https://git-scm.com/) and [UnityHub](https://unity.com/download) installed.

## 1. Clone Repositories

- Clone the DFU repository onto your system
  - `git clone git@github.com:Interkarma/daggerfall-unity.git`
- Change to the mods subdirectory of the DFU repository. This is where all mods will be installed
  - `cd daggerfall-unity/Assets/Game/Mods`
- Clone Cheb's Necromancy. If you forked, use your fork instead of jpw1991
  - `git clone git clone git@github.com:jpw1991/daggerfall-chebs-necromancy.git ChebsNecromancy`

## 2. Configure Unity

- Open UnityHub and go to the installs tab
- If 2019.4.40f1 is missing, install it
- Once 2019.4.40f1 is installed, click `Manage` then `Add Modules`
- Pick whatever targets you want to build for aside from your own system eg. Linux, Mac, Windows.

## 3. Open DFU in 2019.4.40f1

It should open up just fine with no errors. If not, you'll need to seek help beyond the scope of this guide.

- Set up the text editor
  - Click `Edit -> Preferences`
  - In `External Tools -> External Script Editor` pick your editor of choice. I use [Rider](https://www.jetbrains.com/rider/).
- Navigate to one of the mods scripts eg. `Assets/Game/Mods/ChebsNecromancy/ChebsNecromancy.cs` and double click it, your editor should open.
- You can now work on the mod

## 4. Building the Mod

- In Unity click `Daggerfall Tools -> Mod Builder`

## Notes

**Important:** An old C# language version is required because of the way DFU dynamically compiles mods. If you're getting errors like the one below, it is due to code not in compliance with the old version it's expecting:

```
ModManager - started loading mod: Cheb's Necromancy
[Error] [Cheb's Necromancy 0.0.24] <b>Compilation Error CS1525</b>: Unexpected symbol `)', expecting `;' or `}'
	at line#88 column#28 "            }");"
<b>Compilation Error </b>: Unexpected symbol `)', expecting `;' or `}'
	at line#0 column#0
```