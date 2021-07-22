// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const { LuisAdaptiveRecognizer } = require('botbuilder-ai');
const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, BeginDialog, CancelAllDialogs, ConfirmInput, ForEach, IfCondition, OnConversationUpdateActivity, OnIntent, SendActivity, TemplateEngineLanguageGenerator } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');
const { GetUserProfileDialog } = require('../getUserProfileDialog/getUserProfileDialog');

const DIALOG_ID = 'ROOT_DIALOG';

class RootDialog extends ComponentDialog {
    constructor() {
        super(DIALOG_ID);
        const lgFile = Templates.parseFile(path.join(__dirname, 'rootDialog.lg'));
        const dialog = new AdaptiveDialog(DIALOG_ID).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            recognizer: this.createLuisRecognizer(),
            triggers: [
                new OnConversationUpdateActivity(this.welcomeUserSteps()),
                new OnIntent("GetUserProfile", [], [
                    new BeginDialog('GET_USER_PROFILE_DIALOG')
                ]),
                new OnIntent("Help", [], [
                    new SendActivity("${RootHelp()}")
                ]),
                new OnIntent("Cancel", [], [
                    new ConfirmInput().configure({
                        prompt: '${RootCancelConfirm()}',
                        property: 'turn.confirm',
                        allowInterruptions: "false"
                    }),
                    new IfCondition().configure(
                    {
                        condition: "turn.confirm == true",
                        actions: [
                            new SendActivity("${CancelReadBack()}"),
                            new CancelAllDialogs()
                        ],
                        elseActions: [
                            new SendActivity("${Cancelcancelled()}")
                        ]
                    })
                ])
            ]
        });

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.
        this.addDialog(dialog);
        this.addDialog(new GetUserProfileDialog())

        // The initial child Dialog to run.
        this.initialDialogId = DIALOG_ID;
    }

    createLuisRecognizer() {
        if (process.env.rootDialog_en_us_lu === "" || process.env.LuisAPIHostName === "" || process.env.LuisAPIKey === "")
            throw `Sorry, you need to configure your LUIS application and update .env file.`;
        return new LuisAdaptiveRecognizer().configure({
            endpoint: process.env.LuisAPIHostName,
            endpointKey: process.env.LuisAPIKey,
            applicationId: process.env.rootDialog_en_us_lu
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
                            new SendActivity('${WelcomeUser()}')
                        ]
                    })
                ]
            })
        ];
    }
}

module.exports.RootDialog = RootDialog;
