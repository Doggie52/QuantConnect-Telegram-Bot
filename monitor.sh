#!/bin/bash
cd QuantConnect-Telegram-Bot/bin/Release
until mono ./QuantConnect-Telegram-Bot.exe; do
        sleep 1
done