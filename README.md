# QuantConnect Telegram Bot

Unofficial Telegram bot for retrieving data from a QuantConnect live algorithm deployment.

## Getting Started

### Windows

1. Clone into directory of choice.
1. Fill out the [`bot-config.json`](https://github.com/Doggie52/QuantConnect-Telegram-Bot/blob/master/QuantConnect-Telegram-Bot/bot-config.json) file.
1. Restore NuGet packages.
1. Compile and run.

### Google Cloud Compute instance running Ubuntu 18.04 LTS (work in progress)

1. Follow steps to [install Mono](https://www.mono-project.com/download/stable/#download-lin-ubuntu), making sure you install `mono-complete`.
1. `sudo apt update && sudo apt install nuget`
1. Clone into directory of choice, `cd` into this directory.
1. Fill out the [`bot-config.json`](https://github.com/Doggie52/QuantConnect-Telegram-Bot/blob/master/QuantConnect-Telegram-Bot/bot-config.json) file.
1. `nuget restore QuantConnect-Telegram-Bot.sln`
1. `xbuild QuantConnect-Telegram-Bot.sln`
1. `cd QuantConnect-Telegram-Bot/bin/Debug`
1. `mono ./QuantConnect-Telegram-Bot.exe`
