# Unity - Mulligan Rename Tool
Mulligan Rename tool for Unity allows for quick and safe renaming of many assets or GameObjects all at once. It provides several ways to rename Objects including the ability to replace substrings, add prefixes and suffixes, add numbers, and delete characters from the front or back of the name of each Object.

This ReadMe provides a quick overview of the tool. For more detailed documentation, check out the [wiki](https://github.com/redbluegames/unity-bulk-rename/wiki).

![MulliganRenameGIF](https://github.com/redbluegames/unity-bulk-rename/blob/master/ReadMeImages/bulk_rename_gameobjects.gif)

## Installation
To install this package follow these steps:

1. Download the latest package (.unitypackage file) from the [Releases page](https://github.com/redbluegames/unity-bulk-rename/releases) or click [here](https://github.com/redbluegames/unity-bulk-rename/releases/latest).

2. Open the Unity project you want to import the Mulligan Renamer tool into.

3. Install the custom package through Unity's Asset menu.
  - In _Unity Editor_ go to **Assets** -> **Import Package** -> **Custom Package...**
  - Select the .unitypackage file you just downloaded. We recommend storing the files at the default location (Assets/RedBlueGames), but it should work anywhere.

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

## Notes and Considerations
Currently the tool does not allow for undoing Assets, (though Undoing GameObjects in the scene is possible). This seems to be a limitation of Unity's undo system.

## Contributing
Contributing to the project is welcome. You can do so by adding GitHub issues, or by submitting code pull requests. If you submit a pull request, please try to adhere to the StyleCop format and reference the GitHub issue in the Pull Requst comments.


## Contributors and Credits
- [uvivagabond](https://github.com/uvivagabond)
- [lucasrowe](https://github.com/lucasrowe)
- [kevinrmabie](https://github.com/kevinrmabie)
