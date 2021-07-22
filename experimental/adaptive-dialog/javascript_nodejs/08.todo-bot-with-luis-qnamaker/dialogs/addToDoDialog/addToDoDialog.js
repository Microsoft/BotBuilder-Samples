// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { Expression } = require('adaptive-expressions');
const { ActivityFactory } = require('botbuilder');
const { LuisAdaptiveRecognizer, QnAMakerRecognizer } = require('botbuilder-ai');
const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, AdaptiveEvents, ArrayChangeType, CodeAction, CrossTrainedRecognizerSet, EditArray, OnBeginDialog, OnDialogEvent, OnIntent, OnQnAMatch, SendActivity, SetProperty, TemplateEngineLanguageGenerator, TextInput} = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const path = require('path');

// Note: Dialog name needs to match with the file name for cross-train CLI commands and the recognizer to work E2E.
const DIALOG_ID = 'addToDoDialog';

class AddToDoDialog extends ComponentDialog {
    constructor() {
        super(DIALOG_ID);
        const lgFile = Templates.parseFile(path.join(__dirname, `${DIALOG_ID}.lg`));
        this.lgGenerator = new TemplateEngineLanguageGenerator(lgFile);
        const dialog = new AdaptiveDialog(DIALOG_ID).configure({
            generator: this.lgGenerator,
            recognizer: this.createCrossTrainedRecognizer(),
            triggers: [
                new OnBeginDialog([
                    // Take todo title if we already have it from root dialog's LUIS model.
                    // This is the title entity defined in ../RootDialog/RootDialog.lu.
                    // There is one LUIS application for this bot. So any entity captured by the rootDialog
                    // will be automatically available to child dialog.
                    // @EntityName is a short-hand for turn.recognized.entities.<EntityName>. Other useful short-hands are 
                    //     #IntentName is a short-hand for turn.intents.<IntentName>
                    //     $PropertyName is a short-hand for dialog.<PropertyName>
                    new SetProperty().configure(
                        {
                            property: "dialog.itemTitle",
                            value: "=@itemTitle"
                        }),

                    new SetProperty().configure(
                        {
                            property: "dialog.listType",
                            value: "=@listType"
                        }),

                    // TextInput by default will skip the prompt if the property has value.
                    new TextInput().configure(
                        {
                            property: "dialog.itemTitle",
                            prompt: "${GetItemTitle()}",
                            // This entity is coming from the local AddToDoDialog's own LUIS recognizer.
                            // This dialog's .lu file is under \AddToDoDialog\AddToDoDialog.lu
                            value: "=@itemTitle",
                            // Allow interruption if we do not have an item title and have a super high confidence classification of an intent.
                            allowInterruptions: "!@itemTitle && turn.recognized.score >= 0.7"
                        }),
                    // Get list type
                    new TextInput().configure(
                        {
                            property: "dialog.listType",
                            prompt: "${GetListType()}",
                            value: "=@listType",
                            allowInterruptions: "!@listType && turn.recognized.score >= 0.7",
                            validations: [
                                // Verify using expressions that the value is one of todo or shopping or grocery
                                "contains(createArray('todo', 'shopping', 'grocery'), toLower(this.value))",
                            ],
                            outputFormat: "=toLower(this.value)",
                            invalidPrompt: "${GetListType.Invalid()}",
                            maxTurnCount: 2,
                            defaultValue: "todo",
                            defaultValueResponse: "${GetListType.DefaultValueResponse()}"
                        }),
                    // Add the new todo title to the list of todos. Keep the list of todos in the user scope.
                    new EditArray().configure(
                        {
                            itemsProperty: "user.lists[dialog.listType]",
                            changeType: ArrayChangeType.push,
                            value: "=dialog.itemTitle"
                        }),
                    new SendActivity("${AddItemReadBack()}")
                    // All child dialogs will automatically end if there are no additional steps to execute. 
                    // If you wish for a child dialog to not end automatically, you can set 
                    // AutoEndDialog property on the Adaptive Dialog to 'false'
                ]),
                // Although root dialog can handle this, this will match loacally because this dialog's .lu has local definition for this intent. 
                new OnIntent("Help", [], [
                    new SendActivity("${HelpAddItem()}")
                ]),
                // Shows how to use dialog event to capture intent recognition event for more than one intent.
                // Alternate to this would be to add two separate OnIntent events.
                // This ensures we set any entities recognized by these two intents.
                new OnDialogEvent(AdaptiveEvents.recognizedIntent, [
                    new SetProperty().configure(
                        {
                            property: "dialog.itemTitle",
                            value: "=@itemTitle"
                        }),
                    new SetProperty().configure(
                        {
                            property: "dialog.itemTitle",
                            value: "=@itemTitle"
                        }),
                ], "#GetItemTitle || #GetListType"),

                // Help and chitchat is handled by qna
                new OnQnAMatch([
                    // Use code action to render QnA response. This is also a demonstration of how to use code actions to light up custom functionality.
                    new CodeAction(this.resolveAndSendQnAAnswer.bind(this))
                ]),
            ]
        });

        this.addDialog(dialog);

        this.initialDialogId = DIALOG_ID;
    }

    static dialogName = DIALOG_ID;

    createLuisRecognizer() {
        if (process.env.AddToDoDialog_en_us_lu === "" || process.env.LuisAPIHostName === "" || process.env.LuisAPIKey === "")
            throw `Sorry, you need to configure your LUIS application and update .env file.`;
        return new LuisAdaptiveRecognizer().configure({
            endpoint: process.env.LuisAPIHostName,
            endpointKey: process.env.LuisAPIKey,
            applicationId: process.env.AddToDoDialog_en_us_lu,
            // Id needs to be LUIS_<dialogName> for cross-trained recognizer to work.
            id: `LUIS_${DIALOG_ID}`
        });
    }

    createQnARecognizer() {
        if (process.env.TodoBotWithLuisAndQnAJS_en_us_qna === "" || process.env.QnAHostName === "" || process.env.QnAEndpointKey === "")
            throw `Sorry, you need to configure your QnA Maker KB and update .env file.`;
        return new QnAMakerRecognizer().configure({
            hostname: process.env.QnAHostName,
            knowledgeBaseId: process.env.TodoBotWithLuisAndQnAJS_en_us_qna,
            endpointKey: process.env.QnAEndpointKey,
            // Property path where previous qna id is set. This is required to have multi-turn QnA working.
            qnaId: 'turn.qnaIdFromPrompt',
            // Id needs to be QnA_<dialogName> for cross-trained recognizer to work.
            id: `QnA_${DIALOG_ID}`
        });
    }

    createCrossTrainedRecognizer() {
        return new CrossTrainedRecognizerSet().configure({
            recognizers: [
                this.createLuisRecognizer(),
                this.createQnARecognizer()
            ]
        });
    }

    // Code action to process response from QnA maker using the generator for this dialog.
    // You can use code action to perform any operation including memory mutations. 
    resolveAndSendQnAAnswer = async function (dc, options) {
        let exp1 = Expression.parse("@answer").tryEvaluate(dc.state).value;
        let resVal = await this.lgGenerator.generate(dc, exp1, dc.state);
        await dc.context.sendActivity(ActivityFactory.fromObject(resVal));
        return await dc.endDialog(options);
    }

};

module.exports.AddToDoDialog = AddToDoDialog;