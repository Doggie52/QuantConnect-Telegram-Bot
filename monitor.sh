#!/bin/bash
cd QuantConnect-Telegram-Bot/bin/Debug
until mono ./QuantConnect-Telegram-Bot.exe; do
        sleep 1
done