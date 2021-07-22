# SSO with Simple Skill Consumer and Skill

Bot Framework v4 skills echo sample.

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to create a simple root bot that sends message activities to a skill bot that echoes it back.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## Key concepts in this sample

The solution includes a parent bot ([`rootBot`](RootBot/Bots/RootBot.cs)) and a skill bot ([`skillBot`](SkillBot/Bots/SkillBot.cs)) and shows how the skill bot can accept OAuth credentials from the root bot, without needing to send it's own OAuthPrompt.

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
- Create a bot registration in the azure portal for the `SkillBot` and update [SkillBot/appsettings.json](SkillBot/appsettings.json) with the `MicrosoftAppId` and `MicrosoftAppPassword` of the new bot registration.
- Update the `BotFrameworkSkills` section in [RootBot/appsettings.json](RootBot/appsettings.json) with the app ID for the skill you created in the previous step.
- Create a bot registration in the azure portal for the `RootBot` and update [RootBot/appsettings.json](RootBot/appsettings.json) with the `MicrosoftAppId` and `MicrosoftAppPassword` of the new bot registration.
- Add the `RootBot` `MicrosoftAppId` to the `AllowedCallers` list in [SkillBot/appsettings.json](SkillBot/appsettings.json).

- Setup the 2 Azure AD applications for SSO as per steps given in [SkillBot Azure AD](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=srb%2Ccsharp#create-the-azure-ad-identity-application-1) and [RootBot Azure AD](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=sb%2Ccsharp#create-the-azure-ad-identity-application). You will end up with 2 Azure AD applications - one for the skill consumer and one for the skill.
- Create an aadv2 connection in the bot registration for the `SkillBot` and fill in values from the aadv2 application created for SSO, as per the [docs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=srb%2Ccsharp#create-azure-ad-connection-1). Update [SkillBot/appsettings.json](SkillBot/appsettings.json) with the `ConnectionName`  
- Create an aadv2 connection in the bot registration for the `RootBot` and fill in values from the aadv2 application created for SSO, as per the [docs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication-sso?view=azure-bot-service-4.0&tabs=sb%2Ccsharp#create-azure-ad-connection). Update [RootBot/appsettings.json](SkillBot/appsettings.json) with the `ConnectionName`  

- Open the `SSOWithSkills.sln` solution and configure it to [start debugging with multiple processes](https://docs.microsoft.com/en-us/visualstudio/debugger/debug-multiple-processes?view=vs-2019#start-debugging-with-multiple-processes)

## Testing the bot using the Bot Framework Emulator

The [Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.8.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot.
- Enter a Bot URL of `http://localhost:3978/api/messages`, the `MicrosoftAppId` and `MicrosoftAppPassword` for the `RootBot`.
- Click `Connect`.
- Follow the prompts to initiate the token exchange between the `SkillBot` and `RootBot`, resulting in a valid token displayed.

## Deploy the bots to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.
