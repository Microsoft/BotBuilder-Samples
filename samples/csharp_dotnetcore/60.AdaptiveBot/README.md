# AdaptiveBot
Bot Framework v4.5-Preview Adaptive Dialog declarative bot.

This bot has a number of sample dialogs which are defined using declarative .dialog files with .lg and .lu.

## Prerequisites
- [.NET Core SDK][4] version 2.1
	```bash
	# determine dotnet version
	dotnet --version
	```

# To try this sample
- In a terminal, navigate to `AdaptiveBot`
    ```bash
    # change into project folder
	cd # AdaptiveBot
    ```
- Run the bot from a terminal or from Visual Studio, choose option A or B.

	A) From a terminal
	```bash
	# run the bot
	dotnet run
	```

	B) Or from Visual Studio
	- Launch Visual Studio
	- File -> Open -> Project/Solution
	- Navigate to `AdaptiveBot` folder
	- Select `AdaptiveBot.csproj` file
	- Press `F5` to run the project

# Testing the bot using Bot Framework Emulator
[Bot Framework Emulator][5] is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.3.0 or greater from [here][6]

## Connect to the bot using Bot Framework Emulator
- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Preview Nuget feed for the packages this bot is using
https://botbuilder.myget.org/F/botbuilder-declarative/api/v3/index.json 


 
