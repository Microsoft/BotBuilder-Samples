// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const { LuisAdaptiveRecognizer } = require('botbuilder-ai');
const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, CancelAllDialogs, ConfirmInput, DateTimeInput, DeleteProperty, EndDialog, ForEach, IfCondition, OnConversationUpdateActivity, OnIntent, OnUnknownIntent, SendActivity, SetProperties, TemplateEngineLanguageGenerator, TextInput } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const ROOT_DIALOG = 'mainWaterfallDialog';

class RootDialog extends ComponentDialog {
    constructor() {
        super(ROOT_DIALOG);

        const lgFile = Templates.parseFile(path.join(__dirname, 'rootDialog.lg'));
        // There are no steps associated with this dialog.
        // This dialog will react to user input using its own Recognizer's output and Rules.
        const rootDialog = new AdaptiveDialog(ROOT_DIALOG).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            // Add a recognizer to this adaptive dialog.
            // For this dialog, we will use the LUIS recognizer based on the FlightBooking.json
            // found under CognitiveModels folder.
            recognizer: this.createLuisRecognizer(),
            triggers: [
                new OnConversationUpdateActivity(this.welcomeUserSteps()),
                new OnIntent('Greeting', [], [new SendActivity('${BotOverview()}')]),
                new OnIntent('Help', [], [new SendActivity('${BotOverview()}')], '#Help.Score >= 0.8'),
                new OnIntent('Cancel', [], [
                    new SendActivity('Sure, cancelling that...'),
                    new CancelAllDialogs(),
                    new EndDialog()
                ], '#Cancel.Score >= 0.8'),
                new OnUnknownIntent([new SendActivity('${UnknownIntent()}')]),
                new OnIntent('Book_flight', [], [
                    // Save any entities returned by LUIS.
                    new SetProperties().configure({
                        assignments: [
                            {
                                property: 'conversation.flightBooking',
                                value: '={}'
                            },
                            // We will only save any geography city entities that explicitly have been classified as either fromCity or toCity.
                            {
                                property: 'conversation.flightBooking.departureCity',
                                // value is an expression. @entityName is shorthand to refer to the value of an entity recognized.
                                // @xxx is same as turn.recognized.entities.xxx
                                value: '=@fromCity.location'
                            },
                            {
                                property: 'conversation.flightBooking.destinationCity',
                                value: '=@toCity.location'
                            },
                            {
                                property: 'conversation.flightBooking.departureDate',
                                value: '=@datetime.timex[0]'
                            }
                        ]
                    }),
                    // Steps to book A flight.
                    // Help and Cancel intents are always available since TextInput will always initiate
                    // consultation up the parent dialog chain to see if anyone else wants to take the user input.
                    new TextInput().configure({
                        property: 'conversation.flightBooking.departureCity',
                        // Prompt property supports full language generation resolution.
                        // See here to learn more about language generation:
                        // https://aka.ms/language-generation
                        prompt: '${PromptForMissingInformation()}',
                        // We will allow interruptions as long as the user did not explicitly answer the question
                        // This property supports an expression so you can examine presence of an intent via #intentName,
                        //    detect presence of an entity via @entityName etc. Interruption is allowed if the expression
                        //    evaluates to `true`. This property defaults to `true`.
                        allowInterruptions: '!#Book_flight && (!@fromCity || !@geographyV2)',
                        // value is an expression. Take first non-null value.
                        value: '=coalesce(@fromCity.location, @geographyV2.location)'
                    }),
                    // Delete entity so it is not overconsumed as destination as well.
                    new DeleteProperty('turn.recognized.entities.geographyV2'),
                    new TextInput().configure({
                        property: 'conversation.flightBooking.destinationCity',
                        prompt: '${PromptForMissingInformation()}',
                        allowInterruptions: '!#Book_flight && (!@toCity || !@geographyV2)',
                        // value is an expression. Take any recognized city name as fromCity.
                        value: '=coalesce(@toCity.location, @geographyV2.location)'
                    }),
                    new DateTimeInput().configure({
                        property: 'conversation.flightBooking.departureDate',
                        prompt: '${PromptForMissingInformation()}',
                        allowInterruptions: '!#Book_flight && !@datetime',
                        // value is an expression. Take any date time entity recognition as departure date.
                        value: '=@datetime.timex[0]'
                    }),
                    new ConfirmInput().configure({
                        property: 'turn.bookingConfirmation',
                        prompt: '${ConfirmBooking()}',
                        // You can use this flag to control when a specific input participates in consultation bubbling and can be interrupted.
                        // 'false' means interruption is not allowed when this input is active.
                        allowInterruptions: false
                    }),
                    new IfCondition().configure({
                        // All conditions are expressed using the common expression language.
                        // See https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/common-expression-language to learn more
                        condition: 'turn.bookingConfirmation == true',
                        actions: [new SendActivity('${BookingConfirmation()}')],
                        elseActions: [new SendActivity('Thank you.')]
                    })
                ])
            ]
        });

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.
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
                            new SendActivity('${WelcomeCard()}')
                        ]
                    })
                ]
            })
        ];
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
}

module.exports.RootDialog = RootDialog;
