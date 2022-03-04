![MulliganRenameTitle](https://github.com/redbluegames/unity-bulk-rename/blob/main/ReadMeImages/mulligan-github-banner.png)

# Unity - Mulligan Renamer
The Mulligan Renamer tool for Unity allows for quick and safe renaming of many assets or GameObjects all at once. It provides several ways to rename Objects including the ability to replace substrings, add prefixes and suffixes, add numbers, and delete characters from the front or back of the name of each Object.

This ReadMe provides a quick overview of the tool. For more detailed documentation, check out the [wiki](https://github.com/redbluegames/unity-bulk-rename/wiki).

![MulliganRenameGIF](https://github.com/redbluegames/unity-bulk-rename/blob/main/ReadMeImages/mulligan-renamer-overview.gif)

## Installation

### Through Unity Asset Store

<p align="center">
  <a href="https://assetstore.unity.com/packages/slug/99843"><img src="https://github.com/redbluegames/unity-bulk-rename/blob/main/ReadMeImages/readme-asset-store.png" alt="Unity Asset Store"/></a>
</p>

Mulligan Renamer can be installed for free through the [Unity Asset Store](https://assetstore.unity.com/packages/slug/99843).

### Through Github as a UPM Package

In your `Project/Packages/manifest.json`, add the following as a dependency:

```
"com.redbluegames.mulligan": "https://github.com/redbluegames/unity-mulligan-renamer.git?path=/Assets/RedBlueGames/MulliganRenamer"
```

Your file will look something like this:
```
{
  "dependencies": {
    "com.redbluegames.mulligan": "https://github.com/redbluegames/unity-mulligan-renamer.git?path=/Assets/RedBlueGames/MulliganRenamer",
    "com.unity.textmeshpro": "2.1.1",
    "com.unity.timeline": "1.2.17",
    "com.unity.ugui": "1.0.0",
  },
  "testables": [
    "com.unity.inputsystem"
  ]
}
```

Save. When you open Unity it should automatically download Mulligan and add it to your Packages folder in the project.

### Through Github as .unitypackage

To install this package follow these steps:

1. Download the latest package (.unitypackage file) from the [Releases page](https://github.com/redbluegames/unity-bulk-rename/releases) or click [here](https://github.com/redbluegames/unity-bulk-rename/releases/latest).

2. Open the Unity project you want to import the Mulligan Renamer tool into.

3. Install the custom package through Unity's Asset menu.
  - In _Unity Editor_ go to **Assets** -> **Import Package** -> **Custom Package...**
  - Select the .unitypackage file you just downloaded. We recommend storing the files at the default location (Assets/RedBlueGames), but it should work anywhere.

### Through NPM

This package is registered at https://registry.npmjs.org as `com.redbluegames.mulligan`. You can use npm to install it manually, or use Unity Package Manager by adding the following into your `Packages/manifest.json`:
```
{
  "scopedRegistries": [
    {
      "name": "NPM",
      "url": "https://registry.npmjs.org",
      "scopes": [
        "com.redbluegames"
      ]
    }
  ],
  "dependencies": {
    "com.redbluegames.mulligan": "1.7.5"
  }
}
```

## Using the Tool
To use the Mulligan Renamer tool, open it from the `Window/Red Blue` menu. If you have Objects selected,
they will automatically be entered for rename. Otherwise, drag and drop the Assets or GameObjects you want to
rename into the Mulligan Renamer window.

The tool allows for many rename operations. Here are a few:
* **Search String and Replacement String** allow for replacement of substrings from the selected objects.
  * Example: The name "Char_Hero_Idle" with search string "Hero" and Replacement string "Enemy" would yield "Char_Enemy_Idle".
  * The **Regular Expressions** mode for Search and Replace allows for just about any rename operation.
* **Prefix** and **Suffix** additions allow you to add prefixes and suffixes to the start or end of every object.
  * Example: The name "Hero" with the added prefix "Char_" and suffix "_Idle" will yield "Char_Hero_Idle"
* **Trimming** allows you to delete a number of characters from the front or back of the object's name.
  * Example: The name "CHairA" with 1 specified to Delete from Front, and 1 specified to Delete from Back will yield "Hair".
* **Enumerating** allows you to add sequential numbers to the end of each object name. This will be added after the deletions or suffix additions.
  * Example: Selecting 3 objects that are all named "Wall", and specifying "00" or "D2" as the format string, and a Starting Count of 0, will yield "Wall00", "Wall01", and "Wall02".

You can combine any number of the rename operations in any order to achieve the rename results you want. Read more about the operations in the [wiki](https://github.com/redbluegames/unity-bulk-rename/wiki) [here](https://github.com/redbluegames/unity-bulk-rename/wiki/Rename-Operations). 

## Contributing
Contributing to the project is welcome. You can do so by adding GitHub issues, or by submitting code pull requests. Please read the [Contributing Guidelines](https://github.com/redbluegames/unity-mulligan-renamer/blob/main/CONTRIBUTING.md) before contributing.


## Contributors and Credits
**Portuguese Translation and Localization** - [Mukarillo](https://github.com/Mukarillo)

**Spanish Translation** - [Jesús Dávalos](https://github.com/jesus-davalos)

**Spanish Language QA** - [Cristhian García Vélez](https://github.com/crisgarlez)

**Simplified Chinese Translation** - [独行](https://github.com/1401046425)

**Continuous Integration Support** - [andyzickler](https://github.com/andyzickler)

- [uvivagabond](https://github.com/uvivagabond)
- [lucasrowe](https://github.com/lucasrowe)
- [kevinrmabie](https://github.com/kevinrmabie)
