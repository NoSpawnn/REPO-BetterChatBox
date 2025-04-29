# REPO - BetterChatBox

A client-side overhaul of the chat box, making it editable and adding keybind support!

## Keybinds

|                   Key                   | Action                                                                  |
| :-------------------------------------: | ----------------------------------------------------------------------- |
| <kbd>&#8593;</kbd> / <kbd>&#8595;</kbd> | Scroll through message history                                          |
|        <kbd>Delete</kbd> (hold)         | Delete character ahead of cursor (repeat)                               |
|       <kbd>Backspace</kbd> (hold)       | Delete character behind cursor (repeat)                                 |
|           <kbd>Ctrl + X</kbd>           | Delete all text (or cut to clipboard, depending on the config settings) |
|           <kbd>Ctrl + C</kbd>           | Copy all text to the clipboard                                          |
|           <kbd>Ctrl + V</kbd>           | Paste from your clipboard (up to the character limit)                   |
|        <kbd>Ctrl + &#8592;</kbd>        | Move left a word                                                        |
|        <kbd>Ctrl + &#8594;</kbd>        | Move right a word                                                       |
|       <kbd>Ctrl + Backspace</kbd>       | Delete whole word behind cursor                                         |
|        <kbd>Ctrl + Delete</kbd>         | Delete whole word in front of cursor                                    |


## Installation

### Automatic

1. Install through your mod manager
2. Idk what else to tell you...

### Manual

1. Ensure you have BepInEx installed
    - Download BepInEx from [BepInExPack Release](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/)
    - Extract it into your R.E.P.O. game directory

2. Download BetterChatBox
    - Download the latest release from [Thunderstore](https://thunderstore.io/c/repo/p/NoSpawnn/BetterChatBox/)
    - Place the `BetterChatBox.dll` inside `BepInEx/plugins/`

## Configuration

### In game

1. Ensure [REPOConfig](https://thunderstore.io/c/repo/p/nickklmao/REPOConfig/) is installed
2. Launch the game and open the `Mods` menu

### Through Gale

1. Ensure BetterChatBox is installed to the current profile
2. Edit values in the mod config tab

### Manual

1. Open `BepInEx/config/NoSpawnn.BetterChatBox.cfg`
2. Edit the values inside

