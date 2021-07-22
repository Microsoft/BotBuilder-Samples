// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { QnAMakerRecognizer } = require('botbuilder-ai');
const { DialogEvents, ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, DeleteProperty, EmitEvent, ForEach, IfCondition, OnQnAMatch, OnConversationUpdateActivity, OnUnknownIntent, SendActivity, SetProperty, TemplateEngineLanguageGenerator, TextInput } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const path = require('path');

const ROOT_DIALOG = 'ROOT_DIALOG';

class RootDialog extends ComponentDialog {
    constructor() {
        super(ROOT_DIALOG);
        const lgFile = Templates.parseFile(path.join(__dirname, 'rootDialog.lg'));
        const rootDialog = new AdaptiveDialog(ROOT_DIALOG).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            recognizer: this.createQnAMakerRecognizer(),
            triggers: [
                new OnConversationUpdateActivity(this.welcomeUserSteps()),

                // This trigger matches if the response from your QnA KB has follow up prompts.
                new OnQnAMatch([
                    new SetProperty().configure({
                        property: "dialog.qnaContext",
                        value: "=turn.recognized.answers[0].context.prompts"
                    }),
                    new TextInput().configure({
                        prompt: '${ShowMultiTurnAnswer()}',
                        property: 'turn.qnaMultiTurnResponse',
                        // We want the user to respond to the follow up prompt. Do not allow interruptions.
                        allowInterruptions: false,
                        // Since we can have multiple instances of follow up prompts within a single turn, set this to always prompt. 
                        // Alternate to doing this is to delete the 'turn.qnaMultiTurnResponse' property before the EmitEvent.
                        alwaysPrompt: true
                    }),
                    new SetProperty().configure({
                        property: "turn.qnaMatchFromContext",
                        value: "=where(dialog.qnaContext, item, item.displayText == turn.qnaMultiTurnResponse)"
                    }),
                    new DeleteProperty().configure({
                        property: "dialog.qnaContext"
                    }),
                    new IfCondition().configure({
                        condition: "turn.qnaMatchFromContext && count(turn.qnaMatchFromContext) > 0",
                        actions: [
                            new SetProperty().configure({
                                property: "turn.qnaIdFromPrompt",
                                value: "=turn.qnaMatchFromContext[0].qnaId"
                            }),
                            new EmitEvent().configure({
                                eventName: DialogEvents.activityReceived,
                                eventValue: "=turn.activity"
                            })
                        ]
                    })
                ], "count(turn.recognized.answers[0].context.prompts) > 0"),

                // This trigger matches if the response from your QnA KB does not have follow up prompts.
                new OnQnAMatch([
                    new SendActivity("Here is what I have from the KB - ${@Answer}")
                ]),
                new OnUnknownIntent([
                    new SendActivity("${UnknownReadBack()}")
                ])
            ]
        });

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.
        this.addDialog(rootDialog);

        // The initial child Dialog to run.
        this.initialDialogId = ROOT_DIALOG;
    }

    createQnAMakerRecognizer() {
        if (process.env.KnowledgeBaseId === "" || process.env.HostName === "" || process.env.EndpointKey === "")
            throw `Sorry, you need to configure your QnA Maker KB and update .env file.`;
        return new QnAMakerRecognizer().configure({
            hostname: process.env.HostName,
            knowledgeBaseId: process.env.KnowledgeBaseId,
            endpointKey: process.env.EndpointKey,
            // Property path where previous qna id is set. This is required to have multi-turn QnA working.
            qnaId: 'turn.qnaIdFromPrompt',
            // Disable automatically including dialog name as meta data filter on calls to QnA Maker.
            includeDialogNameInMetadata: false
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
