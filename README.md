# QuantConnect Telegram Bot

Unofficial Telegram bot for retrieving data from a QuantConnect live algorithm deployment. Currently requires/supports Oanda accounts.

## Getting Started

### Requirements

 * Windows or *NIX instance
 * (if Windows) Visual Studio 2017
 * QuantConnect account and API key
 * Oanda account and API key
 * Telegram account and Bot API token

### Instructions for installation under Windows

1. Clone into directory of choice.
1. Fill out the [`bot-config.json`](https://github.com/Doggie52/QuantConnect-Telegram-Bot/blob/master/QuantConnect-Telegram-Bot/bot-config.json) file.
1. Restore NuGet packages.
1. Compile and run.

### Instructions for installation under *NIX

1. Follow steps to [install Mono](https://www.mono-project.com/download/stable/#download-lin-ubuntu) for your distro, making sure you install `mono-complete`.
1. `sudo apt-get update && sudo apt-get install nuget`
1. Clone into directory of choice, `cd` into this directory.
1. Fill out the [`bot-config.json`](https://github.com/Doggie52/QuantConnect-Telegram-Bot/blob/master/QuantConnect-Telegram-Bot/bot-config.json) file.
1. `nuget restore QuantConnect-Telegram-Bot.sln`
1. `xbuild QuantConnect-Telegram-Bot.sln`
1. `cd QuantConnect-Telegram-Bot/bin/Debug`
1. `mono ./QuantConnect-Telegram-Bot.exe`

If you want to run the process in the background and have it restart automatically if it crashes, replace the last two steps above with the following:

1. `chmod +x ./monitor.sh`
1. `nohup ./monitor.sh &`
