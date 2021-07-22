# Todo sample

This sample demonstrates using [Adaptive dialog][1],  [Language Generation][2] PREVIEW features with [LUIS][5] to demonstrate an end-to-end todo bot with support for interruptions.

## Prerequisites

- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    # determine node version
    node --version
    ```

## To try this sample

- Clone the repository

    ```bash
    git clone https://github.com/microsoft/botbuilder-samples.git
    ```

- In a terminal, navigate to `experimental/adaptive-dialog/javascript_nodejs/06.todo-bot`

    ```bash
    cd experimental/adaptive-dialog/javascript_nodejs/06.todo-bot
    ```

- Install modules

    ```bash
    npm install
    ```

- Start the bot

    ```bash
    npm start
    ```

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.3.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## LUIS Setup
### Using LUIS portal
- Navigate and sign in to [Luis.ai][5]
- Under "My apps", click on "Import new app"
- Click on "Choose app file (JSON format) ..."
- Select `botbuilder-samples/experimental/adaptive-dialog/javascript_nodejs/todo-bot/CognitiveModels/ToDoLuisBot.luis.json
- Once the application is imported
    - Click on 'Train' to train the application
    - Click on 'Publish' to publish the application.
- Update appsettings.json
    - You can get your 'Authoring key' by following instructions [here][9]
    - You can get your application id and endpoint region by following instructions [here][10]

### Using CLI
- Install [nodejs][2] version 10.14 or higher
- Install required CLI tools
```bash
> npm i -g @microsoft/botframework-cli
```
- In a command prompt, navigate to `botbuilder-samples/experimental/adaptive-dialog/javascript_nodejs/todo-bot`
- To parse rootDialog.lu to a LUIS json model
```bash
> bf luis:convert --in ./dialogs/rootDialog/rootDialog.lu --out ./CognitiveModels/ToDoLuisBot.luis.json --force
```
- To create a new LUIS application using this model. Note: You see [here][9] for instructions on getting your authoirng key.
```bash
> luis import application --in ./CognitiveModels/ToDoLuisBot.luis.json --authoringKey <YOUR-AUTHORING-KEY>
```
- Copy the relevant Application Id, endpoint information as well as your authoring key to appsettings.json.
- To train and publish the LUIS application,
```bash
> luis train version --appId <YOUR-APP-ID> --versionId 0.1 --wait --authoringKey <YOUR-AUTHORING-KEY>
> luis publish version --appId <YOUR-APP-ID> --versionId 0.1 --wait --authoringKey <YOUR-AUTHORING-KEY>
```

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Dialogs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0)
- [Gathering Input Using Prompts](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0&tabs=csharp)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
- [Restify](https://www.npmjs.com/package/restify)
- [dotenv](https://www.npmjs.com/package/dotenv)

[1]:https://aka.ms/adaptive-dialogs
[2]:https://aka.ms/language-generation
[4]:https://www.npmjs.com/search?q=botbuilder
