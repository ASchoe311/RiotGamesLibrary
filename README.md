![Dynamic JSON Badge](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fapi.github.com%2Frepos%2FASchoe311%2FRiotGamesLibrary%2Freleases%2Flatest&query=%24.assets%5B0%5D.download_count&label=Latest%20Release%20Downloads%20)    ![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/ASchoe311/RiotGamesLibrary/total?label=Lifetime%20Downloads)


# [Riot Games](https://riotgames.com/) library plugin for [Playnite](https://playnite.link/)

Allows Playnite to manage install, uninstall, and launching of games by Riot Games

Can attach companion applications to League of Legends and Valorant (e.g. U.GG, Porofesser, blitz.gg) that launch and close with the games

Enables Riot Client to be force closed when a game is closed so it isn't always running in the background

## Installation

Download the latest .pext file from releases and open it using Playnite

## Usage

Upon installing the addon, refresh game library. The addon should auto-detect any existing installations of Riot Games properties.

Open the Riot Games settings under the addons menu to select the executables for your companion apps and choose if they should or shouldn't close with their related games.

To install games, the plugin will open the Riot client where you must manually select and install the desired game. This is due to Riot not providing a method to select a game to install via command line arguments or other automateable methods.
