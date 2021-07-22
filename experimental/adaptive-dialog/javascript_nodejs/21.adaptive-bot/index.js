// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const dotenv = require('dotenv');
const path = require('path');
const restify = require('restify');
const { BotFrameworkAdapter, ComponentRegistration, ConversationState, MemoryStorage, UserState, useBotState } = require('botbuilder');
const { DialogManager, ListStyle } = require('botbuilder-dialogs');
const { AdaptiveComponentRegistration, AdaptiveDialog, CancelAllDialogs, Case, OnIntent, RegexRecognizer, TemplateEngineLanguageGenerator, ChoiceInput, SendActivity, SwitchCondition, RepeatDialog, OnBeginDialog, LanguageGeneratorExtensions, ResourceExtensions } = require('botbuilder-dialogs-adaptive');
const { ResourceExplorer } = require('botbuilder-dialogs-declarative');
const { Templates } = require('botbuilder-lg');

// Import required bot configuration.
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

// Set up resource explorer
const resourceExplorer = new ResourceExplorer().addFolder(path.join(__dirname, './dialogs'), true, true);

// Add adaptive dialog assets.
ComponentRegistration.add(new AdaptiveComponentRegistration());

// Create HTTP server
const server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`\n${server.name} listening to ${server.url}`);
    console.log('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator');
    console.log('\nTo talk to your bot, open the emulator select "Open Bot"');
});

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about how bots work.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

// Catch-all for errors.
const onTurnErrorHandler = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Send a message to the user
    await context.sendActivity('The bot encountered an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
};

// Set the onTurnError for the singleton BotFrameworkAdapter.
adapter.onTurnError = onTurnErrorHandler;

// Define the state store for your bot.
// See https://aka.ms/about-bot-state to learn more about using MemoryStorage.
// A bot requires a state storage system to persist the dialog and user state between messages.
const memoryStorage = new MemoryStorage();

// Create conversation state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);
useBotState(adapter, conversationState, userState);

let myBot;

function createChoiceInputForAllAdaptiveDialogs() {
    const rootDialog = new AdaptiveDialog(AdaptiveDialog.name).configure({
        recognizer: new RegexRecognizer().configure({
            intents: [{
                intent: 'cancel',
                pattern: 'cancel'
            }]
        })
    });
    const choices = [];
    const switchCases = [];
    (resourceExplorer.getResources('.dialog') || []).forEach(resource => {
        if (resource.id !== undefined && resource.id.endsWith('.main.dialog')) {
            let dialogName = path.basename(resource.id, '.main.dialog');
            const subDialog = resourceExplorer.loadType(resource);
            choices.push({ value: dialogName });
            switchCases.push(new Case(dialogName, [subDialog]));
        }
    });
    const lgFile = Templates.parseFile(path.join(__dirname, './index.lg'));
    rootDialog.generator = new TemplateEngineLanguageGenerator(lgFile);
    rootDialog.triggers.push(new OnBeginDialog([
        new ChoiceInput().configure({
            property: 'turn.userChoice',
            prompt: "${PickADialog()}",
            style: ListStyle.list,
            choices: choices,
            alwaysPrompt: true
        }),
        new SendActivity("${RunningSampleReadBack()}"),
        new SwitchCondition('turn.userChoice', [], switchCases),
        new RepeatDialog()
    ]));
    rootDialog.triggers.push(new OnIntent('cancel', [], [
        new SendActivity("Sure, cancelling that.. Say something to start over.."),
        new CancelAllDialogs()
    ]))
    return rootDialog;
}

function loadRootDialog() {
    console.log('(Re)Loading root dialog...')
    // Load root dialog
    myBot = new DialogManager(createChoiceInputForAllAdaptiveDialogs());
    ResourceExtensions.useResourceExplorer(myBot, resourceExplorer);
    LanguageGeneratorExtensions.useLanguageGeneration(myBot);
}

loadRootDialog();

// Listen for incoming requests.
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        // Route to main dialog.
        await myBot.onTurn(context);
    });
});

// Listen for Upgrade requests for Streaming.
server.on('upgrade', (req, socket, head) => {
    // Create an adapter scoped to this WebSocket connection to allow storing session data.
    const streamingAdapter = new BotFrameworkAdapter({
        appId: process.env.MicrosoftAppId,
        appPassword: process.env.MicrosoftAppPassword
    });
    // Set onTurnError for the BotFrameworkAdapter created for each connection.
    streamingAdapter.onTurnError = onTurnErrorHandler;

    streamingAdapter.useWebSocket(req, socket, head, async (context) => {
        // After connecting via WebSocket, run this logic for every request sent over
        // the WebSocket connection.
        await myBot.onTurn(context);
    });
});

// Add a resource change handler to resource explorer.
resourceExplorer.changed = (resources) => {
    if ((resources || []).find(r => r.id.endsWith('.dialog')) !== undefined) {
        loadRootDialog();
    }
};
