// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { Expression } = require('adaptive-expressions');
const { ActivityFactory } = require('botbuilder');
const { LuisAdaptiveRecognizer, QnAMakerRecognizer } = require('botbuilder-ai');
const { ComponentDialog, ListStyle} = require('botbuilder-dialogs');
const { AdaptiveDialog, AdaptiveEvents, ArrayChangeType, ChoiceInput, ChoiceOutputFormat, CodeAction, CrossTrainedRecognizerSet, EditArray, EndDialog, IfCondition, OnBeginDialog, OnDialogEvent, OnQnAMatch, SendActivity, SetProperty, TemplateEngineLanguageGenerator, TextInput} = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const path = require('path');

// Note: Dialog name needs to match with the file name for cross-train CLI commands and the recognizer to work E2E.
const DIALOG_ID = 'deleteToDoDialog';

class DeleteToDoDialog extends ComponentDialog {
    constructor() {
        super(DIALOG_ID);
        const lgFile = Templates.parseFile(path.join(__dirname, `${DIALOG_ID}.lg`));
        this.lgGenerator = new TemplateEngineLanguageGenerator(lgFile);
        const dialog = new AdaptiveDialog(DIALOG_ID).configure({
            generator: this.lgGenerator,
            recognizer: this.createCrossTrainedRecognizer(),
            triggers: [
                new OnBeginDialog([
                    // Handle case where there are no items in todo list
                    new IfCondition().configure(
                        {
                            // All conditions are expressed using the common expression language.
                            // See https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/common-expression-language to learn more
                            condition: "count(user.lists.todo) == 0 && count(user.lists.grocery) == 0 && count(user.lists.shopping) == 0",
                            actions: [
                                new SendActivity("${DeleteEmptyList()}"),
                                new EndDialog()
                            ]
                        }),

                    // User could have specified the item and/ or list type to delete.
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


                    // Ask for list type first.
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

                    new IfCondition().configure(
                        {
                            condition: "count(user.lists[dialog.listType]) == 0",
                            actions: [
                                new SendActivity("${NoItemsInList()}"),
                                new EndDialog()
                            ]
                        }),

                    // Ask for title to delete
                    new ChoiceInput().configure(
                        {
                            choices: "user.lists[dialog.listType]",
                            property: "dialog.itemTitle",
                            outputFormat: ChoiceOutputFormat.value,
                            style: ListStyle.list,
                            prompt: "${GetItemTitleToDelete()}"
                        }),

                    // remove item
                    new EditArray().configure(
                        {
                            itemsProperty: "user.lists[dialog.listType]",
                            value: "=dialog.itemTitle",
                            changeType: ArrayChangeType.remove
                        }),

                    new SendActivity("${DeleteConfirmationReadBack()}")
                ]),
                // Shows how to use dialog event to capture intent recognition event for more than one intent.
                // Alternate to this would be to add two separate OnIntent events.
                // This ensures we set any entities recognized by these two intents.
                new OnDialogEvent(AdaptiveEvents.recognizedIntent, [
                    new SetProperty().configure(
                        {
                            property: "dialog.itemTitle",
                            value: "=@itemTitle"
                        }
                    ),
                    new SetProperty().configure(
                        {
                            property: "dialog.listType",
                            value: "=@listType"
                        }
                    )
                ], "#GetTitleToDelete || #GetListType"),

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
        if (process.env.DeleteToDoDialog_en_us_lu === "" || process.env.LuisAPIHostName === "" || process.env.LuisAPIKey === "")
            throw `Sorry, you need to configure your LUIS application and update .env file.`;
        return new LuisAdaptiveRecognizer().configure({
            endpoint: process.env.LuisAPIHostName,
            endpointKey: process.env.LuisAPIKey,
            applicationId: process.env.DeleteToDoDialog_en_us_lu,
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
            qnaId: "turn.qnaIdFromPrompt",
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

module.exports.DeleteToDoDialog = DeleteToDoDialog;