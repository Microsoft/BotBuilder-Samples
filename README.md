
# ![Bot Framework Samples](./docs/media/BotFrameworkSamples_header.png)

## [Click here to find out what's new with Bot Framework](https://github.com/microsoft/botframework-sdk/blob/master/README.md)

## Overview

This branch contains samples for the released version of the **Microsoft Bot Framework V4 SDK** for [.NET](https://github.com/Microsoft/botbuilder-dotnet), [JS](https://github.com//microsoft/botbuilder-js) and [Python](https://github.com//microsoft/botbuilder-python). If you need samples for the Bot Framework _V3_ SDK, go [here](https://github.com/Microsoft/BotBuilder-Samples/tree/v3-sdk-samples).

## Getting the samples

To use the samples, clone this GitHub repository using Git.

```bash
    git clone https://github.com/Microsoft/BotBuilder-Samples.git
    cd BotBuilder-Samples
```

## Sample lists

Samples are designed to illustrate functionality you'll need to implement to build great bots!

- [Bot essentials](#bot-essentials)
- [Advanced bots](#advanced-bots)
- [Authentication samples](#authentication-samples)
- [QnA Maker samples](#qna-maker-samples)
- [Teams samples](#teams-samples)
- [Skills samples](#skills-samples)
- [Adaptive Dialog samples](#adaptive-dialog-samples)
- [Custom adapter samples](#custom-adapter-samples)
- [Experimental / preview samples](#experimental--preview-samples)

### Bot essentials

|    | Sample Name           | Description                                                                      | .NET    | JavaScript   | Python  | Java |
|:--:|:----------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:--------|:--------|
|2|Echo bot             | Demonstrates how to receive and send messages.                                 |[.NET&nbsp;Core][cs#2] |[JavaScript][js#2], [TypeScript][ts#2]|[Python][py#2]|[Java][java#2]
|3|Welcome user         | Introduces activity types and provides a welcome message on conversation update activity. |[.NET&nbsp;Core][cs#3] |[JavaScript][js#3], [TypeScript][ts#3]|[Python][py#3]|[Java][java#3]
|5|Multi&nbsp;turn&nbsp;prompts    | Demonstrates how to use waterfall dialog, prompts, and component dialog to create a simple interaction that asks the user for name, age, and prints back that information.          |[.NET&nbsp;Core][cs#5] |[JavaScript][js#5], [TypeScript][ts#5] |[Python][py#5]|[Java][java#5]
|6|Using cards          | Introduces all card types including thumbnail, audio, media etc. Builds on Welcoming user + multi-prompt bot by presenting a card with buttons in welcome message that route to appropriate dialog.     |[.NET&nbsp;Core][cs#6] |[JavaScript][js#6], [TypeScript][ts#6] |[Python][py#6]|[Java][java#6]
|7|Adaptive cards | Demonstrates how the multi-turn dialog can use a card to get user input for name and age. |[.NET&nbsp;Core][cs#7] |[JavaScript][js#7] |[Python][py#7]|[Java][java#7]
|8|Suggested actions    | Demonstrates how to enable your bot to present buttons that the user can tap to provide input.                                      |[.NET&nbsp;Core][cs#8] |[JavaScript][js#8] |[Python][py#8]|[Java][java#8]
|13|Core bot            | Core bot shows how to use cards, dialog, and Language Understanding (LUIS).                         |[.NET&nbsp;Core][cs#13], [.NET&nbsp;Web][wa#13]|[JavaScript][js#13], [TypeScript][ts#13]|[Python][py#13]|[Java][java#13]
|14|NLP with dispatch   | Demonstrates how to dispatch across LUIS and QnA Maker.                            |[.NET&nbsp;Core][cs#14]|[JavaScript][js#14]|[Python][py#14]|[Java][java#14]
|15|Handling&nbsp;attachments| Demonstrates how to listen for/handle user provided attachments.                |[.NET&nbsp;Core][cs#15]|[JavaScript][js#15]|[Python][py#15]|[Java][java#15]
|40|TIMEX resolution    | Demonstrates various ways to parse and manipulate the TIMEX expressions you get from LUIS and the [DateTimeRecognizer](https://github.com/Microsoft/recognizers-text) used by the DateTimePrompt. |[.NET&nbsp;Core][cs#40] |[JavaScript][js#40]|[Python][py#40]|[Java][java#40]
|43|Complex dialogs      | Demonstrates different ways for composing dialogs. |[.NET&nbsp;Core][cs#43]|[JavaScript][js#43] |[Python][py#43]|[Java][java#43]
|45|State management    | Demonstrates how to use state management and storage objects to manage and persist state. | [.NET&nbsp;Core][cs#45] | [JavaScript][js#45]   |[Python][py#45]|[Java][java#45]

### Advanced bots

|    | Sample Name           | Description                                                                      | .NET    | JavaScript   | Python  |  Java |
|:--:|:----------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:--------|:--------|
|1|Console&nbsp;echo&nbsp;bot         | Introduces the concept of adapter and demonstrates a simple echo bot on console adapter and how to send a reply and access the incoming message.           |[.NET&nbsp;Core][cs#1] |[JavaScript][js#1], [TypeScript][ts#1] |[Python][py#1]
|1|Browser echo bot         | Demonstrates how to host a bot in the browser using Web Chat and a custom Web Chat Adapter.   |  | [ECMAScript&nbsp;6][es#1]  |
|16|Proactive&nbsp;messages  | Demonstrates how to send proactive messages to users.                           |[.NET&nbsp;Core][cs#16]|[JavaScript][js#16], [TypeScript][ts#16]|[Python][py#16]|[Java][java#16]
|17|Multilingual bot    | Using translate middleware to support a multi-lingual bot. Demonstrates custom middleware. |[.NET&nbsp;Core][cs#17]|[JavaScript][js#17]|[Python][py#17]|[Java][java#17]
|19|Custom dialogs      | Demonstrates complex conversation flow using the Dialogs library. |[.NET&nbsp;Core][cs#19]|[JavaScript][js#19]|[Python][py#19]|[Java][java#19]
|21|Application&nbsp;Insights      | Demonstrates how to add telemetry logging to your bot, storing telemetry within Application Insights.|[.NET&nbsp;Core][cs#21] |[JavaScript][js#21] ||[Java][java#21]
|23|Facebook events     | Integrate and consume Facebook specific payloads, such as post-backs, quick replies and opt-in events.|[.NET&nbsp;Core][cs#23] |[JavaScript][js#23] |[Python][py#23]|[Java][java#23]
|42|Scale out            | Demonstrates how you can build your own state solution from the ground up that supports scaled out deployment with ETag based optimistic locking. |[.NET&nbsp;Core][cs#42] |    |[Python][py#42]|[Java][java#42]
|44|Basic custom prompts | Demonstrates how to implement your own _basic_ prompts to ask the user for information. |[.NET&nbsp;Core][cs#44]|[JavaScript][js#44]|[Python][py#44]|[Java][java#44]
|47|Inspection&nbsp;middleware    | Demonstrates how to use middleware to allow the Bot Framework Emulator to debug traffic into and out of the bot in addition to looking at the current state of the bot. | [.NET&nbsp;Core][cs#47] | [JavaScript][js#47]   |[Python][py#47]|[Java][java#47]
|70|Styling webchat     | This sample shows how to create a web page with custom Web Chat component.|         | [ECMAScript&nbsp;6][es#70]         |

### Authentication samples

|    | Sample Name           | Description                                                                      | .NET    | JavaScript   | Python  | Java |
|:--:|:----------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:--------|:--------|
|18|OAuth authentication  | Bot that demonstrates how to integrate OAuth providers.                  |[.NET&nbsp;Core][cs#18]|[JavaScript][js#18]|[Python][py#18]|[Java][java#18]
|24|MSGraph&nbsp;authentication    | Demonstrates bot authentication capabilities of Azure Bot Service. Demonstrates utilizing the Microsoft Graph API to retrieve data about the user.|[.NET&nbsp;Core][cs#24] |[JavaScript][js#24] |[Python][py#24]|[Java][java#24]
|46|Teams authentication    | Demonstrates how to use authentication for a bot running in Microsoft Teams. | [.NET&nbsp;Core][cs#46] | [JavaScript][js#46]   |[Python][py#46]|[Java][java#46]

### QnA Maker samples

|    | Sample Name           | Description                                                                      | .NET    | JavaScript   | Python  | Java |
|:--:|:----------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:--------|:--------|
|11|QnA Maker (simple)             | Demonstrates how to use QnA Maker to have simple single-turn conversations     |[.NET&nbsp;Core][cs#11]|[JavaScript][js#11]      |[Python][py#11]|[Java][java#11]
|49|QnA&nbsp;Maker&nbsp;(advanced) | Demonstrates how to integrate Multiturn and Active learning in a QnA Maker bot.  This also demonstrates the QnAMakerDialog class. |[.NET&nbsp;Core][cs#49]|[JavaScript][js#49]     | |[Java][java#49]

### Teams samples

|    | Sample Name           | Description                                                                      | .NET    | JavaScript   | Python  | Java |
|:--:|:----------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:--------|:--------|
|25| Message Reactions | Demonstrates how to create a simple bot that responds to Message Reactions | [.NET&nbsp;Core][cs#25]  |  [JavaScript][js#25] ||[Java][java#25]
|46|Authentication    | Demonstrates how to use authentication for a bot running in Microsoft Teams. | [.NET&nbsp;Core][cs#46] | [JavaScript][js#46]   |[Python][py#46] |[Java][java#46]
|50|Messaging&nbsp;extensions&nbsp;- search     |  A Messaging Extension that accepts search requests and returns results.|[.NET&nbsp;Core][cs#50]|[JavaScript][js#50]          |[Python][py#50] |[Java][java#50]
|51|Messaging&nbsp;extensions&nbsp;- action     |  A Messaging Extension that accepts parameters and returns a card.  Also, how to receive a forwarded message as a parameter in a Messaging Extension.|[.NET&nbsp;Core][cs#51]|[JavaScript][js#51]          |[Python][py#51] |[Java][java#51]
|52|Messaging&nbsp;extensions&nbsp;- auth and config     | A Messaging Extension that has a configuration page, accepts search requests and returns results after the user has signed in.|[.NET&nbsp;Core][cs#52]|[JavaScript][js#52]          | |[Java][java#52]
|53|Messaging&nbsp;extensions&nbsp;- action preview     | Demonstrates how to create a Preview and Edit flow for a Messaging Extension.|[.NET&nbsp;Core][cs#53]|[JavaScript][js#53]          |[Python][py#53] |[Java][java#53]
|54|Task module     | Demonstrates how to retrieve a Task Module, and values from cards in the Task Module, for a Messaging Extension.|[.NET&nbsp;Core][cs#54]|[JavaScript][js#54]          |[Python][py#54] |[Java][java#54]
|55|Link unfurling     | A Messaging Extension that performs link unfurling.|[.NET&nbsp;Core][cs#55]|[JavaScript][js#55]          |[Python][py#55] |[Java][java#55]
|56|File upload     | Demonstrates how to obtain file consent, and upload files to Teams from a bot. Also, how to receive a file sent to a bot.|[.NET&nbsp;Core][cs#56]|[JavaScript][js#56]          |[Python][py#56] |[Java][java#56]
|57|Conversation bot     | Demonstrates various features of bots on Teams: message all members in a Team or Channel, @mention a user from a bot, update previously sent messages, etc. |[.NET&nbsp;Core][cs#57]|[JavaScript][js#57]          |[Python][py#57] |[Java][java#57]
|58|Start new thread in a channel     | Demonstrates creating a new thread in a channel. |[.NET&nbsp;Core][cs#58]|[JavaScript][js#58]          |[Python][py#58] |[Java][java#58]

### Custom adapter samples

|    | Sample Name           | Description                                                   | .NET                    | JavaScript  | Python            | Java |
|:--:|:----------------------|:--------------------------------------------------------------|:------------------------|:------------|:------------------|:------------------|
|60|Slack       | Demonstrates the use of the custom adapter for Slack.         | [.NET&nbsp;Core][cs#60] |             | [Python][py#60]   |
|61|Facebook    | Demonstrates the use of the custom adapter for Facebook.      | [.NET&nbsp;Core][cs#61] |             |                   |
|62|Webex       | Demonstrates the use of the custom adapter for Webex Teams.   | [.NET&nbsp;Core][cs#62] |             |                   |
|63|Twilio      | Demonstrates the use of the custom adapter for Twilio.        | [.NET&nbsp;Core][cs#63] |             |                   |

### Skills samples

|    | Sample Name           | Description                                                                      | .NET    | JavaScript   | Python  | Java
|:--:|:----------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:--------|:--------
|80|Skills&nbsp;-&nbsp;simple&nbsp;bot&nbsp;to&nbsp;bot | This sample shows how to connect a skill to a skill consumer.       | [.NET&nbsp;Core][cs#80] | [JavaScript][js#80]     |[Python][py#80] |[Java][java#80]
|81|Skills - skill dialog       | This sample shows how to connect a skill to a skill dialog consumer.| [.NET&nbsp;Core][cs#81] | [JavaScript][js#81]     |[Python][py#81] |[Java][java#81]

### Adaptive dialog samples

|    | Sample Name           | Description                                                                                                                                                                                                                  | .NET                          | JavaScript                   | Python  |
|:--:|:----------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:------------------------------|:-----------------------------|:--------|
|01| Multi turn prompt                              | This sample demonstrates using [Adaptive dialog][doc#1] and [Language Generation][doc#2] features to achieve the same functionality that the waterfall based multi-turn-prompt sample [here][cs#5].   | [.NET&nbsp;Core][cs#82]       | [JavaScript (Preview)][js#82]|    
|02| Using Cards                                    | This sample demonstrates using [Adaptive dialog][doc#1] and [Language Generation][doc#2] features to achieve the same functionality that the using card sample [here][cs#6].                          | [.NET&nbsp;Core][cs#83]       | [JavaScript (Preview)][js#83]|    
|03| Core Bot                                       | This sample demonstrates using [Adaptive dialog][doc#1] and [Language Generation][doc#2] features to achieve the same functionality that the waterfall based core bot sample [here][cs#13].           | [.NET&nbsp;Core][cs#84]       | [JavaScript (Preview)][js#84]|    
|04| Waterfall or custom dialog with adaptive dialog | This sample starts with [19.custom-dialogs][cs#19] sample and shows how to get adaptive dialogs and custom/waterfall dialogs work together.                                                          | [.NET&nbsp;Core][cs#85]       | [JavaScript (Preview)][js#85]|    
|05| Interruptions bot                              | Demostrate how to create a bot that use [LUIS][doc#3] and adaptive card to achieve advance LU concepts such as interruption handling and flexible entity entraction.                                  | [.NET&nbsp;Core][cs#86]       | [JavaScript (Preview)][js#86]|
|06| Todo bot                                       | Demostrate the use of [Adaptive dialog][doc#1], [Language Generation][doc#2] features with [LUIS][doc#3] to demostrate an end-to-end ToDo bot in action including support for interruptions.          | [.NET&nbsp;Core][cs#87]       | [JavaScript (Preview)][js#87]|
|07| QnA Maker                                      | This sample demostrate how to consume [QnA Maker Knowledge Base][doc#4] within your [Adaptive dialog][doc#1] based bot.                                                                               | [.NET&nbsp;Core][cs#88]       | [JavaScript (Preview)][js#88]|
|08| Todo bot with LUIS and QnA Maker               | This sample demostrate the use of [Adaptive dialog][doc#1], [Language Generation][doc#2] features with [LUIS][doc#3] and [QnA Maker][doc#4] to demostrate an end-to-end ToDo bot in action.           | [.NET&nbsp;Core][cs#89]       | [JavaScript (Preview)][js#89]|
|09| Integrating Composer Dialogs                   | This bot sample show you how to integrate dialogs generated by [Bot Framework Composer][doc#5] to your existing V4 SDK based bot.                                                                     | [.NET&nbsp;Core][cs#90]       | [JavaScript (Preview)][js#90]|
|20| Echo bot declarative                           | This bot sample shows how to use declarative form of [Adaptive dialog][doc#1].                                                                                                                        | [.NET&nbsp;Core][cs#91]       | [JavaScript (Preview)][js#91]|
|21| Adaptive bot declarative                       | This bot sample demostrates various declarative defined [Adaptive dialog][doc#1].                                                                                                                     | [.NET&nbsp;Core][cs#92]       | [JavaScript (Preview)][js#92]|

### Experimental / preview samples

A [collection of **experimental** samples](./experimental) exist, intended to provide samples for features currently in preview or as a way to solicit feedback on a given design, approach, or technology being considered by the Bot Framework Team.

<!--- Doc reference -->
[doc#1]:https://aka.ms/adaptive-dialogs
[doc#2]:https://aka.ms/language-generation
[doc#3]:https://luis.ai/
[doc#4]:https://qnamaker.ai/
[doc#5]:https://github.com/microsoft/botframework-composer
<!--- End Doc reference -->

<!--- CSharp sample reference -->
[cs#1]:samples/csharp_dotnetcore/01.console-echo
[cs#2]:samples/csharp_dotnetcore/02.echo-bot
[cs#3]:samples/csharp_dotnetcore/03.welcome-user
[cs#5]:samples/csharp_dotnetcore/05.multi-turn-prompt
[cs#6]:samples/csharp_dotnetcore/06.using-cards
[cs#7]:samples/csharp_dotnetcore/07.using-adaptive-cards
[cs#8]:samples/csharp_dotnetcore/08.suggested-actions
[cs#11]:samples/csharp_dotnetcore/11.qnamaker
[cs#12]:samples/csharp_dotnetcore/11a.qnamaker
[cs#13]:samples/csharp_dotnetcore/13.core-bot
[cs#13.b]:samples/csharp_dotnetcore/13.core-bot.tests
[cs#14]:samples/csharp_dotnetcore/14.nlp-with-orchestrator
[cs#15]:samples/csharp_dotnetcore/15.handling-attachments
[cs#16]:samples/csharp_dotnetcore/16.proactive-messages
[cs#17]:samples/csharp_dotnetcore/17.multilingual-bot
[cs#18]:samples/csharp_dotnetcore/18.bot-authentication
[cs#19]:samples/csharp_dotnetcore/19.custom-dialogs
[cs#21]:samples/csharp_dotnetcore/21.corebot-app-insights
[cs#23]:samples/csharp_dotnetcore/23.facebook-events
[cs#24]:samples/csharp_dotnetcore/24.bot-authentication-msgraph
[cs#25]:samples/csharp_dotnetcore/25.message-reaction
[cs#40]:samples/csharp_dotnetcore/40.timex-resolution
[cs#42]:samples/csharp_dotnetcore/42.scaleout
[cs#43]:samples/csharp_dotnetcore/43.complex-dialog
[cs#44]:samples/csharp_dotnetcore/44.prompt-users-for-input
[cs#45]:samples/csharp_dotnetcore/45.state-management
[cs#46]:samples/csharp_dotnetcore/46.teams-auth
[cs#47]:samples/csharp_dotnetcore/47.inspection
[cs#49]:samples/csharp_dotnetcore/49.qnamaker-all-features
[cs#50]:samples/csharp_dotnetcore/50.teams-messaging-extensions-search
[cs#51]:samples/csharp_dotnetcore/51.teams-messaging-extensions-action
[cs#52]:samples/csharp_dotnetcore/52.teams-messaging-extensions-search-auth-config
[cs#53]:samples/csharp_dotnetcore/53.teams-messaging-extensions-action-preview
[cs#54]:samples/csharp_dotnetcore/54.teams-task-module
[cs#55]:samples/csharp_dotnetcore/55.teams-link-unfurling
[cs#56]:samples/csharp_dotnetcore/56.teams-file-upload
[cs#57]:samples/csharp_dotnetcore/57.teams-conversation-bot
[cs#58]:samples/csharp_dotnetcore/58.teams-start-new-thread-in-channel
[cs#60]:samples/csharp_dotnetcore/60.slack-adapter
[cs#61]:samples/csharp_dotnetcore/61.facebook-adapter
[cs#62]:samples/csharp_dotnetcore/62.webex-adapter
[cs#63]:samples/csharp_dotnetcore/63.twilio-adapter
[cs#80]:samples/csharp_dotnetcore/80.skills-simple-bot-to-bot
[cs#81]:samples/csharp_dotnetcore/81.skills-skilldialog
[cs#82]:samples/csharp_dotnetcore/adaptive-dialog/01.multi-turn-prompt
[cs#83]:samples/csharp_dotnetcore/adaptive-dialog/02.using-cards
[cs#84]:samples/csharp_dotnetcore/adaptive-dialog/03.core-bot
[cs#85]:samples/csharp_dotnetcore/adaptive-dialog/04.waterfall-or-custom-dialog-with-adaptive
[cs#86]:samples/csharp_dotnetcore/adaptive-dialog/05.interruptions-bot
[cs#87]:samples/csharp_dotnetcore/adaptive-dialog/06.todo-bot
[cs#88]:samples/csharp_dotnetcore/adaptive-dialog/07.qnamaker
[cs#89]:samples/csharp_dotnetcore/adaptive-dialog/08.todo-bot-luis-qnamaker
[cs#90]:samples/csharp_dotnetcore/adaptive-dialog/09.integrating-composer-dialogs
[cs#91]:samples/csharp_dotnetcore/adaptive-dialog/20.EchoBot-declarative
[cs#92]:samples/csharp_dotnetcore/adaptive-dialog/21.AdaptiveBot-declarative
<!--- End CSharp sample reference -->

<!--- CSharp Web Api sample reference -->
[wa#13]:samples/csharp_webapi/13.core-bot
<!--- End CSharp Api sample reference -->

<!--- JavaScript ES6 sample reference -->
[es#1]:samples/javascript_es6/01.browser-echo
[es#70]:samples/javascript_es6/70.styling-webchat
<!--- End JavaScript ES6 sample reference -->

<!--- TypeScript sample reference -->
[ts#0]:samples/typescript_nodejs/00.empty-bot
[ts#1]:samples/typescript_nodejs/01.console-echo
[ts#2]:samples/typescript_nodejs/02.echo-bot
[ts#3]:samples/typescript_nodejs/03.welcome-users
[ts#5]:samples/typescript_nodejs/05.multi-turn-prompt
[ts#6]:samples/typescript_nodejs/06.using-cards
[ts#13]:samples/typescript_nodejs/13.core-bot
[ts#16]:samples/typescript_nodejs/16.proactive-messages
[ts#52]:https://github.com/Microsoft/AI/tree/master/templates/Enterprise-Template
<!--- End TypeScript sample reference -->

<!--- JavaScript sample reference -->
[js#1]:samples/javascript_nodejs/01.console-echo
[js#2]:samples/javascript_nodejs/02.echo-bot
[js#3]:samples/javascript_nodejs/03.welcome-users
[js#5]:samples/javascript_nodejs/05.multi-turn-prompt
[js#6]:samples/javascript_nodejs/06.using-cards
[js#7]:samples/javascript_nodejs/07.using-adaptive-cards
[js#8]:samples/javascript_nodejs/08.suggested-actions
[js#11]:samples/javascript_nodejs/11.qnamaker
[js#12]:samples/javascript_nodejs/11a.qnamaker
[js#13]:samples/javascript_nodejs/13.core-bot
[js#14]:samples/javascript_nodejs/14.nlp-with-orchestrator
[js#15]:samples/javascript_nodejs/15.handling-attachments
[js#16]:samples/javascript_nodejs/16.proactive-messages
[js#17]:samples/javascript_nodejs/17.multilingual-bot
[js#18]:samples/javascript_nodejs/18.bot-authentication
[js#19]:samples/javascript_nodejs/19.custom-dialogs
[js#21]:samples/javascript_nodejs/21.corebot-app-insights
[js#23]:samples/javascript_nodejs/23.facebook-events
[js#24]:samples/javascript_nodejs/24.bot-authentication-msgraph
[js#25]:samples/javascript_nodejs/25.message-reaction
[js#40]:samples/javascript_nodejs/40.timex-resolution
[js#43]:samples/javascript_nodejs/43.complex-dialog
[js#44]:samples/javascript_nodejs/44.prompt-for-user-input
[js#45]:samples/javascript_nodejs/45.state-management
[js#46]:samples/javascript_nodejs/46.teams-auth
[js#47]:samples/javascript_nodejs/47.inspection
[js#49]:samples/javascript_nodejs/49.qnamaker-all-features
[js#50]:samples/javascript_nodejs/50.teams-messaging-extensions-search
[js#51]:samples/javascript_nodejs/51.teams-messaging-extensions-action
[js#52]:samples/javascript_nodejs/52.teams-messaging-extensions-search-auth-config
[js#53]:samples/javascript_nodejs/53.teams-messaging-extensions-action-preview
[js#54]:samples/javascript_nodejs/54.teams-task-module
[js#55]:samples/javascript_nodejs/55.teams-link-unfurling
[js#56]:samples/javascript_nodejs/56.teams-file-upload
[js#57]:samples/javascript_nodejs/57.teams-conversation-bot
[js#58]:samples/javascript_nodejs/58.teams-start-new-thread-in-channel
[js#80]:samples/javascript_nodejs/80.skills-simple-bot-to-bot
[js#81]:samples/javascript_nodejs/81.skills-skilldialog
[js#82]:experimental/adaptive-dialog/javascript_nodejs/01.multi-turn-prompt
[js#83]:experimental/adaptive-dialog/javascript_nodejs/02.using-cards:
[js#84]:experimental/adaptive-dialog/javascript_nodejs/03.core-bot
[js#85]:experimental/adaptive-dialog/javascript_nodejs/04.waterfall-or-custom-dialog-with-adaptive
[js#86]:experimental/adaptive-dialog/javascript_nodejs/05.interruptions-bot
[js#87]:experimental/adaptive-dialog/javascript_nodejs/06.todo-bot
[js#88]:experimental/adaptive-dialog/javascript_nodejs/07.qnamaker
[js#89]:experimental/adaptive-dialog/javascript_nodejs/08.todo-bot-with-luis-qnamaker
[js#90]:experimental/adaptive-dialog/javascript_nodejs/09.integrating-composer-dialogs
[js#91]:experimental/adaptive-dialog/javascript_nodejs/20.echo-bot-declarative
[js#92]:experimental/adaptive-dialog/javascript_nodejs/21.adaptive-bot
<!--- End JavaScript sample reference -->

<!--- Python sample reference -->
[py#1]:samples/python/01.console-echo
[py#2]:samples/python/02.echo-bot
[py#3]:samples/python/03.welcome-user
[py#5]:samples/python/05.multi-turn-prompt
[py#6]:samples/python/06.using-cards
[py#7]:samples/python/07.using-adaptive-cards
[py#8]:samples/python/08.suggested-actions
[py#11]:samples/python/11.qnamaker
[py#13]:samples/python/13.core-bot
[py#14]:samples/python/14.nlp-with-dispatch
[py#15]:samples/python/15.handling-attachments
[py#16]:samples/python/16.proactive-messages
[py#17]:samples/python/17.multilingual-bot
[py#18]:samples/python/18.bot-authentication
[py#19]:samples/python/19.custom-dialogs
[py#21]:samples/python/21.corebot-app-insights
[py#23]:samples/python/23.facebook-events
[py#24]:samples/python/24.bot-authentication-msgraph
[py#40]:samples/python/40.timex-resolution
[py#42]:samples/python/42.scaleout
[py#43]:samples/python/43.complex-dialog
[py#44]:samples/python/44.prompt-for-user-input
[py#45]:samples/python/45.state-management
[py#46]:samples/python/46.teams-auth
[py#47]:samples/python/47.inspection
[py#50]:samples/python/50.teams-messaging-extensions-search
[py#51]:samples/python/51.teams-messaging-extensions-action
[py#52]:samples/python/52.teams-messaging-extensions-search-auth-config
[py#53]:samples/python/53.teams-messaging-extensions-action-preview
[py#54]:samples/python/54.teams-task-module
[py#55]:samples/python/55.teams-link-unfurling
[py#56]:samples/python/56.teams-file-upload
[py#57]:samples/python/57.teams-conversation-bot
[py#58]:samples/python/58.teams-start-thread-in-channel
[py#60]:samples/python/60.slack-adapter
[py#80]:samples/python/80.skills-simple-bot-to-bot
[py#81]:samples/python/81.skills-skilldialog
<!--- End Python sample reference -->

[java#2]:samples/java_springboot/02.echo-bot
[java#3]:samples/java_springboot/03.welcome-user
[java#5]:samples/java_springboot/05.multi-turn-prompt
[java#6]:samples/java_springboot/06.using-cards
[java#7]:samples/java_springboot/07.using-adaptive-cards
[java#8]:samples/java_springboot/08.suggested-actions
[java#11]:samples/java_springboot/11.qnamaker
[java#13]:samples/java_springboot/13.core-bot
[java#14]:samples/java_springboot/14.nlp-with-dispatch
[java#15]:samples/java_springboot/15.handling-attachments
[java#16]:samples/java_springboot/16.proactive-messages
[java#17]:samples/java_springboot/17.multilingual-bot
[java#18]:samples/java_springboot/18.bot-authentication
[java#19]:samples/java_springboot/19.custom-dialogs
[java#21]:samples/java_springboot/21.corebot-app-insights
[java#23]:samples/java_springboot/23.facebook-events
[java#24]:samples/java_springboot/24.bot-authentication-msgraph
[java#25]:samples/java_springboot/25.message-reaction
[java#40]:samples/java_springboot/40.timex-resolution
[java#42]:samples/java_springboot/42.scaleout
[java#43]:samples/java_springboot/43.complex-dialog
[java#44]:samples/java_springboot/44.prompt-users-for-input
[java#45]:samples/java_springboot/45.state-management
[java#46]:samples/java_springboot/46.teams-auth
[java#47]:samples/java_springboot/47.inspection
[java#49]:samples/java_springboot/49.qnamaker-all-features
[java#50]:samples/java_springboot/50.teams-messaging-extensions-search
[java#51]:samples/java_springboot/51.teams-messaging-extensions-action
[java#52]:samples/java_springboot/52.teams-messaging-extensions-search-auth-config
[java#53]:samples/java_springboot/53.teams-messaging-extensions-action-preview
[java#54]:samples/java_springboot/54.teams-task-module
[java#55]:samples/java_springboot/55.teams-link-unfurling
[java#56]:samples/java_springboot/56.teams-file-upload
[java#57]:samples/java_springboot/57.teams-conversation-bot
[java#58]:samples/java_springboot/58.teams-start-new-thread-in-channel
[java#80]:samples/java_springboot/80.skills-simple-bot-to-bot
[java#81]:samples/java_springboot/81.skills-skilldialog

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Reporting security issues

Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) at [secure@microsoft.com](mailto:secure@microsoft.com). You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the [MSRC PGP](https://technet.microsoft.com/en-us/security/dn606155) key, can be found in the [Security TechCenter](https://technet.microsoft.com/en-us/security/default).

Copyright (c) Microsoft Corporation. All rights reserved.
