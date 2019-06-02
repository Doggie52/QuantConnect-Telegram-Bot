FROM mono:latest

# Stage 0: Get prerequisites
RUN apt-get update && apt-get --yes install unzip wget nuget msbuild

# Stage 1: Download the project
RUN \
	wget https://github.com/Doggie52/QuantConnect-Telegram-Bot/archive/master.zip && \
	mkdir -p ~/src && \
	unzip master.zip -d ~/src

WORKDIR /root/src/QuantConnect-Telegram-Bot-master

# Stage 2: Build the project
RUN \
	nuget restore QuantConnect-Telegram-Bot.sln && \
	msbuild QuantConnect-Telegram-Bot.sln /p:Configuration=Release

# Stage 3: Copy configuration to Release folder
COPY QuantConnect-Telegram-Bot/bot-config.json QuantConnect-Telegram-Bot/bin/Release/bot-config.json
RUN cat QuantConnect-Telegram-Bot/bin/Release/bot-config.json

# Stage 4: Run the project on deploy
CMD cd QuantConnect-Telegram-Bot/bin/Release && mono ./QuantConnect-Telegram-Bot.exe
