# How to Contribute #

Thanks for your interest in contributing to the project! Please follow these suggestions to make sure we all work efficiently together.

## Submitting an Issue ##
Before submitting an issue, give a quick search to see if the issue is already logged. For new issues, feel free to format the Issue however you like. We will add any necessary labels, or reformat it if it will help with organization. If you are submitting a feature request, a mockup would be incredibly helpful to quickly understand the request.

## Coding Guidelines ##

1. It is very helpful to first submit an issue for whatever you'd like to see added to the base repository. 
This way you can reference the issue in your commits.
1. [Fork](https://help.github.com/articles/fork-a-repo/) the repository and make your change in your fork.
1. In general, try to adhere to the style of the files you are working in. This is most important for whitespace. But the official coding guidelines we follow are the [Aviva Solutions coding guidelines](https://csharpcodingguidelines.com/). 
1. If you're using MonoDevelop you can install the StyleCop plugin [here](http://addins.monodevelop.com/Project/Index/54) to help enforce Style Guidelines.
1. All contributions should be licensed to match the base repository and include the standard boilerplate.
1. Please use [well-formed git commit messages](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html)
1. Submit a [pull request](https://help.github.com/articles/creating-a-pull-request) for your change.

## Issue Management ##
When starting work on an issue, follow these steps:

New Contributors:
1. Claim the issue: Comment on the issue that you are investigating it, so others will know it's claimed.
2. When complete, submit a Pull Request into the development branch.

Frequent Contributors:
1. Assign the issue to yourself.
2. If the issue is in a Project, move it to To Do.
3. When complete, submit a Pull Request into the development branch. A repository admin will move it to Done when it's approved and merged into the developoment branch.

Issues are "Closed" only once they are merged into main and released.

If you'd like to be added to the Frequent Contributors team here: https://github.com/orgs/redbluegames/teams/frequent-collaborators

## Branching Strategy / Pull Requests ##

There are three branches to know about:
* main
* staging
* develop-v1.X.Y

Main should reflect live code, which is pushed to the Releases and also Unity Asset store.
Staging is where we push code before going live. This is mainly useful for updating languages, as we can add the languages to staging and then request them with debugs in Mulligan.
Develop branch is where we work out of before merging them into staging and creating a release. Pull requests should be made into the develop branch.

If there is no develop branch pushed, it means there is no patch currently in development. In this event, pull requests can be made into staging.

## Adding a Language ##

To Add a language:
1. Duplicate the English file (en.json), which is located in the Resources subfolder inside the Mulligan folder.
1. In the new file, replace all its fields for the given new language.
1. You should be able to switch to the new language in the Mulligan preferences as soon as you've created it. You can use this to verify your strings in the tool as you go.
1. Verify tests.
1. After you've finished, merge into staging.
1. Find the link to the new language file in the respository and add that into the LanguageBookmarks.json file in root of the directory. This lets the Preferences lookup find the file.
1. Once the bookmarks are set, you should be able to delete your local language file, and download it by holding SHIFT while pressing "Update Language" in preferences.
1. Merge to main.
1. Update LanguageBookmarks.json to point to the language file on Main.
