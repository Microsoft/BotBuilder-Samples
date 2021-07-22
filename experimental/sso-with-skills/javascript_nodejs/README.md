# SSO with Simple Skill Consumer and Skill

Bot Framework v4 Skills SSO Skills sample.

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to create a simple root bot that sends message activities to a skill bot that echoes it back.

## Prerequisites

- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    # determine node version
    node --version

## Key concepts in this sample

The solution includes a parent bot ([`rootBot`](rootBot/bots/rootBot.js)) and a skill bot ([`skillBot`](skillBot/bots/skillBot.js)) and shows how the skill bot can accept OAuth credentials from the root bot, without needing to send it's own OAuthPrompt.

This is the general authentication flow :

1. Root bot prompts user to authenticate with an OAuth prompt card.
2. Authentication succeeds and the user is granted a token.
3. User performs and action on the skill bot that requires authentication.
4. The skill bot sends an OAuth prompt card to the root bot.
5. The root bot intercepts the OAuth prompt card, aware that the user is already authenticated and that the user should authenticate with the skill via SSO.
6. Instead of showing the OAuth prompt card to the user, the root bot sends a token exchange request invoke activity along with the token to the skill.
7. The skill's OAuth prompt receives the token exchange request and uses the token from the root bot to continue authenticating.

## To try this sample

- Clone the repository

    ```bash
    git clone https://github.com/microsoft/botbuilder-samples.git
    ```

- Create a bot registration in the azure portal for the `skillBot` and update [skillBot/.env](skillBot/.env) with the `MicrosoftAppId` and `MicrosoftAppPassword` of the new bot registration.
- Create a bot registration in the azure portal for the `rootBot` and update [rootBot/.env](rootBot/.env) with the `MicrosoftAppId` and `MicrosoftAppPassword` of the new bot registration.
- Update the `SkillAppId` variable in [rootBot/.env](rootBot/.env) with the `AppId` for the skill you created in the previous step.
- (Optionally) Add the `rootBot` `MicrosoftAppId` to the `AllowedCallers` comma-separated list in [skillBot/.env](skillBot/.env).

- Setup the 2 Azure AD applications for SSO as per steps given in [SkillBot Azure AD](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=srb%2Ccsharp#create-the-azure-ad-identity-application-1) and [RootBot Azure AD](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=sb%2Ccsharp#create-the-azure-ad-identity-application). You will end up with 2 Azure AD applications - one for the skill consumer and one for the skill.
- Create an Azure AD v2 connection in the bot registration for the `SkillBot` and fill in values from the Azure AD v2 application created for SSO, as per the [docs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=srb%2Ccsharp#create-azure-ad-connection-1). Update [SkillBot/appsettings.json](SkillBot/appsettings.json) with the `ConnectionName`.
- Create an Azure AD v2 connection in the bot registration for the `RootBot` and fill in values from the Azure AD v2 application created for SSO, as per the [docs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=sb%2Ccsharp#create-azure-ad-connection). Update [RootBot/appsettings.json](SkillBot/appsettings.json) with the `ConnectionName`.

- In a terminal, navigate to `samples\javascript_nodejs\82.sso-with-skills\skillBot`.

    ```bash
    cd samples\javascript_nodejs\82.sso-with-skills\skillBot
    ```

- Install npm modules and start the bot.

    ```bash
    npm install
    npm start
    ```

- Open a *second* terminal window and navigate to `samples\javascript_nodejs\82.sso-with-skills\rootBot`.

    ```bash
    cd samples\javascript_nodejs\82.sso-with-skills\rootBot
    ```

- Install npm modules and start the bot.

    ```bash
    npm install
    npm start
    ```

## Testing the bot using the Bot Framework Emulator

The [Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.9.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using the Bot Framework Emulator

- Launch the Bot Framework Emulator.
- Select **File** > **Open Bot**.
- Enter a Bot URL of `http://localhost:3978/api/messages`, and fill in the **Microsoft App ID** and **Microsoft App password** for the root bot.
- Select **Connect**.
- Follow the prompts to initiate the token exchange between the skill bot and root bot. The bot will display a valid token.

## Deploy the bots to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.
