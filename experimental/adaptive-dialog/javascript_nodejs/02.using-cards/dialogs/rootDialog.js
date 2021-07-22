// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const { ComponentDialog, ListStyle } = require('botbuilder-dialogs');
const { AdaptiveDialog, Case, ChoiceInput, ForEach, IfCondition, OnConversationUpdateActivity, OnUnknownIntent, RepeatDialog, SendActivity, SwitchCondition, TemplateEngineLanguageGenerator } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const ROOT_DIALOG = 'rootDialog';

class RootDialog extends ComponentDialog {
    constructor() {
        super(ROOT_DIALOG);

        const lgFile = Templates.parseFile(path.join(__dirname, 'rootDialog.lg'));
        const rootDialog = new AdaptiveDialog(ROOT_DIALOG).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            triggers: [
                new OnConversationUpdateActivity(this.welcomeUserSteps()),
                // Respond to user on message activity
                new OnUnknownIntent(this.onBeginDialogSteps())
            ]
        });

        this.addDialog(rootDialog);
        // The initial child Dialog to run.
        this.initialDialogId = ROOT_DIALOG;
    }

    welcomeUserSteps() {
        // Iterate through membersAdded list and greet user added to the conversation.
        return [
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

    onBeginDialogSteps() {
        return [
            new ChoiceInput().configure({
                property: 'turn.userChoice',
                prompt: '${CardChoice()}',
                style: ListStyle.auto,
                choices: this.getChoices(),
                alwaysPrompt: true
            }),
            new SwitchCondition('turn.userChoice', [new SendActivity('${AllCards()}')], this.getSwitchCases()),
            new RepeatDialog()
        ];
    }

    getSwitchCases() {
        return [
            new Case('Adaptive Card', [new SendActivity('${AdaptiveCard()}')]),
            new Case('Animation Card', [new SendActivity('${AnimationCard()}')]),
            new Case('Audio Card', [new SendActivity('${AudioCard()}')]),
            new Case('Hero Card', [new SendActivity('${HeroCard()}')]),
            new Case('Signin Card', [new SendActivity('${SigninCard()}')]),
            new Case('Thumbnail Card', [new SendActivity('${ThumbnailCard()}')]),
            new Case('Video Card', [new SendActivity('${VideoCard()}')]),
            new Case('All Cards', [new SendActivity('${AllCards()}')])
        ];
    }

    getChoices() {
        const cardOptions = [
            {
                value: 'Adaptive Card',
                synonyms: ['adaptive']
            },
            {
                value: 'Animation Card',
                synonyms: ['animation']
            },
            {
                value: 'Audio Card',
                synonyms: ['audio']
            },
            {
                value: 'Hero Card',
                synonyms: ['hero']
            },
            {
                value: 'Signin Card',
                synonyms: ['signin']
            },
            {
                value: 'Thumbnail Card',
                synonyms: ['thumbnail', 'thumb']
            },
            {
                value: 'Video Card',
                synonyms: ['video']
            },
            {
                value: 'All Cards',
                synonyms: ['all']
            }
        ];

        return cardOptions;
    }
}

module.exports.RootDialog = RootDialog;
