# Unity - Bulk Rename Tool
Bulk Rename tool for Unity allows for quick and safe renaming of many assets or GameObjects.

## Installation
To install this package follow these steps:
1. Download the latest package from the Releases page.

2. Open the Unity project you want to import the Bulk Rename tool into.

3. Install the custom package through Unity's Asset menu. 
  - In _Unity Editor_ go to **Assets** -> **Import Package** -> **Custom Package...**
  - Select the .unitypackage file you just downloaded. We recommend storing the files at the default location (Assets/RedBlueGames), but it should work anywhere.
  
## Using the Tool
To use the Bulk Rename tool you must first select the assets or GameObjects in a scene you want to rename. Right click, and select "Rename in Bulk". This will open up the rename utility. You can continue to select or deselect Objects while the tool is open.

The tool allows for the following options:
* **Search String and Replacement String** allow for replacement of substrings from the selected objects.
 * Example: The name "Char_Hero_Idle" with search string "Hero" and Replacement string "Enemy" would yield "Char_Enemy_Idle".
* **Prefix** and **Suffix** additions allow you to add prefixes and suffixes to the start or end of every object.
 * Example: The name "Hero" with the added prefix "Char_" and suffix "_Idle" will yield "Char_Hero_Idle"
* **Trimming** allows you to delete a number of characters from either the front or the back of the object's name.
 * Example: The name "CHairA" with 1 specified to Delete from Front, and 1 specified to Delete from Back with yield "Hair".
* **Enumerating** allows you to add sequential numbers to the end of each object name. This will be added after the deletions or suffix additions.
 * Example: Selecting 3 objects that are all named "Wall", and specifying "00" or "D2" as the format string, and a Starting Count of 0, will yield "Wall00", "Wall01", and "Wall02".

You can use some or all of these options when renaming your assets.

Below these options you will see a preview of the results, and a Rename button, which will rename the Objects.

## Notes and Considerations
Currently the tool does not allow for undoing Assets, (though Undoing GameObjects in the scene is possible). This seems to be a limitation of Unity's undo system.

## Contributing
Contributing to the project is welcome. You can do so by adding GitHub issues, or by submitting code pull requests. If you submit a pull request, please try to adhere to the StyleCop format and reference the GitHub issue in the Pull Requst comments.
