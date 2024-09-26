![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/ASchoe311/RiotGamesLibrary/latest/total?label=Latest%20Release%20Downloads)    ![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/ASchoe311/RiotGamesLibrary/total?label=Lifetime%20Downloads)
[![Crowdin](https://badges.crowdin.net/riot-games-library/localized.svg)](https://crowdin.com/project/riot-games-library)


# [Riot Games](https://riotgames.com/) library plugin for [Playnite](https://playnite.link/)

Allows Playnite to manage install, uninstall, and launching of games by Riot Games

Can attach companion applications to League of Legends and Valorant (e.g. U.GG, Porofesser, blitz.gg) that launch and close with the games

Enables Riot Client to be force closed when a game is closed so it isn't always running in the background

### Please contribute translations on [Crowdin](https://crowdin.com/project/riot-games-library)!

## Installation

Download the latest .pext file from releases and open it using Playnite

## Usage

Upon installing the addon, refresh game library. The addon should auto-detect any existing installations of Riot Games properties.

Open the Riot Games settings under the addons menu to select the executables for your companion apps and choose if they should or shouldn't close with their related games.

To install games, the plugin will open the Riot client where you must manually select and install the desired game. This is due to Riot not providing a method to select a game to install via command line arguments or other automateable methods.
