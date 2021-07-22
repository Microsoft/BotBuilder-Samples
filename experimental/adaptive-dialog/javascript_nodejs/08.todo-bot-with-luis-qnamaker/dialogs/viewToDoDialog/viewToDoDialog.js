// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const { LuisAdaptiveRecognizer, QnAMakerRecognizer } = require('botbuilder-ai');
const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, CrossTrainedRecognizerSet, IfCondition, OnBeginDialog, OnQnAMatch, SendActivity, TemplateEngineLanguageGenerator, TextInput } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const DIALOG_ID = 'viewToDoDialog';

class ViewToDoDialog extends ComponentDialog {
    constructor() {
        super(DIALOG_ID);
        const lgFile = Templates.parseFile(path.join(__dirname, 'viewToDoDialog.lg'));
        const dialog = new AdaptiveDialog(DIALOG_ID).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            recognizer: this.createCrossTrainedRecognizer(),
            triggers: [
                new OnBeginDialog([
                    // See if any list has any items.
                    new IfCondition().configure(
                        {
                            condition: "count(user.lists.todo) != 0 || count(user.lists.grocery) != 0 || count(user.lists.shopping) != 0",
                            actions: [
                                // Get list type
                                new TextInput().configure(
                                    {
                                        property: "dialog.listType",
                                        prompt: "${GetListType()}",
                                        value: "=@listType",
                                        allowInterruptions: "!@listType && turn.recognized.score >= 0.7",
                                        validations: [
                                            // Verify using expressions that the value is one of todo or shopping or grocery
                                            "contains(createArray('todo', 'shopping', 'grocery', 'all'), toLower(this.value))",
                                        ],
                                        outputFormat: "=toLower(this.value)",
                                        invalidPrompt: "${GetListType.Invalid()}",
                                        maxTurnCount: 2,
                                        defaultValue: "todo",
                                        defaultValueResponse: "${GetListType.DefaultValueResponse()}"
                                    }),
                                new SendActivity("${ShowList()}")
                            ],
                            elseActions: [
                                new SendActivity("${NoItemsInLists()}")
                            ]
                        }),
                ]),
                // Help and chitchat is handled by qna
                new OnQnAMatch([
                    new SendActivity("${@Answer}")
                ])
            ]
        });

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.
        this.addDialog(dialog);

        // The initial child Dialog to run.
        this.initialDialogId = DIALOG_ID;
    }

    static dialogName = DIALOG_ID;

    createLuisRecognizer() {
        if (process.env.ViewToDoDialog_en_us_lu === "" || process.env.LuisAPIHostName === "" || process.env.LuisAPIKey === "")
            throw `Sorry, you need to configure your LUIS application and update .env file.`;
        return new LuisAdaptiveRecognizer().configure({
            endpoint: process.env.LuisAPIHostName,
            endpointKey: process.env.LuisAPIKey,
            applicationId: process.env.ViewToDoDialog_en_us_lu,
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

};

module.exports.ViewToDoDialog = ViewToDoDialog;