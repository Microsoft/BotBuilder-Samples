// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { LuisAdaptiveRecognizer } = require('botbuilder-ai');
const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, BeginDialog, CancelAllDialogs, ForEach, IfCondition, OnConversationUpdateActivity, OnIntent, SendActivity, TemplateEngineLanguageGenerator } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const { AddToDoDialog } = require('../addToDoDialog/addToDoDialog');
const { DeleteToDoDialog } = require('../deleteToDoDialog/deleteToDoDialog');
const { ViewToDoDialog } = require('../viewToDoDialog/viewToDoDialog');

const path = require('path');

const ROOT_DIALOG = 'ROOT_DIALOG';

class RootDialog extends ComponentDialog {
    constructor() {
        super(ROOT_DIALOG);
        const lgFile = Templates.parseFile(path.join(__dirname, 'rootDialog.lg'));
        const rootDialog = new AdaptiveDialog(ROOT_DIALOG).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            recognizer: this.createLuisRecognizer(),
            triggers: [
                new OnConversationUpdateActivity(this.welcomeUserSteps()),
                new OnIntent('Greeting', [], [new SendActivity('${HelpRootDialog()}')]),
                new OnIntent('AddToDoDialog', [], [new BeginDialog('ADD_TO_DO_DIALOG')], '#AddToDoDialog.Score >= 0.5'),
                new OnIntent('DeleteToDoDialog', [], [new BeginDialog('DELETE_TO_DO_DIALOG')], '#DeleteToDoDialog.Score >= 0.5'),
                new OnIntent('ViewToDoDialog', [], [new BeginDialog('VIEW_TO_DO_DIALOG')], '#ViewToDoDialog.Score >= 0.5'),
                // Come back with LG template based readback for global help
                new OnIntent('Help', [], [new SendActivity('${HelpRootDialog()}')], '#Help.Score >= 0.8'),
                new OnIntent('Cancel', [], [
                    // This is the global cancel in case a child dialog did not explicit handle cancel.
                    new SendActivity('Cancelling all dialogs..'),
                    // SendActivity supports full language generation resolution.
                    // See here to learn more about language generation
                    // https://aka.ms/language-generation
                    new SendActivity('${WelcomeActions()}'),
                    new CancelAllDialogs()
                ], '#Cancel.Score >= 0.8')
            ]
        });

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.
        this.addDialog(rootDialog);

        // Add all child dialogs
        this.addDialog(new AddToDoDialog());
        this.addDialog(new DeleteToDoDialog());
        this.addDialog(new ViewToDoDialog());

        // The initial child Dialog to run.
        this.initialDialogId = ROOT_DIALOG;
    }

    createLuisRecognizer() {
        if (process.env.LuisAppId === "" || process.env.LuisAPIHostName === "" || process.env.LuisAPIKey === "")
            throw `Sorry, you need to configure your LUIS application and update .env file.`;
        return new LuisAdaptiveRecognizer().configure({
            endpoint: process.env.LuisAPIHostName,
            endpointKey: process.env.LuisAPIKey,
            applicationId: process.env.LuisAppId
        });
    }

    welcomeUserSteps() {
        return [
            // Iterate through membersAdded list and greet user added to the conversation.
            new ForEach().configure({
                itemsProperty: 'turn.activity.membersAdded',
                actions: [
                    // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                    // Filter cases where the bot itself is the recipient of the message.
                    new IfCondition().configure({
                        condition: '$foreach.value.name != turn.activity.recipient.name',
                        actions: [
                            new SendActivity('${IntroMessage()}')
                        ]
                    })
                ]
            })
        ];
    }
}

module.exports.RootDialog = RootDialog;
