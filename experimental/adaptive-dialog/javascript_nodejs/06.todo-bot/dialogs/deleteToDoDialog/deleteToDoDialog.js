// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, ArrayChangeType, BeginDialog, CodeAction, DeleteProperty, EditArray, EndDialog, IfCondition, OnBeginDialog, RepeatDialog, SendActivity, TemplateEngineLanguageGenerator, TextInput } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const path = require('path');

const DIALOG_ID = 'DELETE_TO_DO_DIALOG';

class DeleteToDoDialog extends ComponentDialog {
    constructor() {
        super(DIALOG_ID);
        const lgFile = Templates.parseFile(path.join(__dirname, 'deleteToDoDialog.lg'));
        const dialog = new AdaptiveDialog(DIALOG_ID).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            triggers: [
                new OnBeginDialog([
                    // Handle case where there are no items in todo list
                    new IfCondition().configure({
                        // All conditions are expressed using the common expression language.
                        // See https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/common-expression-language to learn more
                        condition: 'user.todos == null || count(user.todos) <= 0',
                        actions: [
                            new SendActivity('${DeleteEmptyList()}'),
                            new SendActivity('${WelcomeActions()}'),
                            new EndDialog()
                        ]
                    }),
                    // User could have already specified the todo to delete via
                    // todoTitle as simple machine learned LUIS entity or
                    // todoTitle_patternAny as pattern.any LUIS entity .or.
                    // prebuilt number entity that denotes the position of the todo item in the list .or.
                    // todoIdx machine learned entity that can detect things like first or last etc.

                    // As a demonstration for this example, use a code step to understand entities returned by LUIS.
                    // You could have easily replaced the code step with these two steps
                    // new SaveEntity('@todoTitle[0]', 'turn.todoTitle'),
                    // new SaveEntity('@todoTitle_patternAny[0]', 'turn.todoTitle'),
                    new CodeAction(this.getToDoTitleToDelete),
                    new IfCondition().configure({
                        condition: 'turn.todoTitle == null',
                        actions: [
                            // First show the current list of Todos
                            new BeginDialog('VIEW_TO_DO_DIALOG'),
                            new TextInput().configure({
                                property: 'turn.todoTitle',
                                prompt: '${GetToDoTitleToDelete()}',
                                // Allow interruptions enable interruptions while the user is in the middle of this prompt
                                // The value to allow interruptions is an expression so you can examine any property to decide if
                                // interruptions are allowed or not. In this sample, we are not allowing interruptions
                                allowInterruptions: false
                            })
                        ]
                    }),
                    new IfCondition().configure({
                        condition: 'contains(user.todos, turn.todoTitle) == false',
                        actions: [
                            new SendActivity('${TodoNotFound()}'),
                            new DeleteProperty().configure({
                                property: 'turn.todoTitle'
                            }),
                            new RepeatDialog()
                        ]
                    }),
                    new EditArray().configure({
                        itemsProperty: 'user.todos',
                        changeType: ArrayChangeType.remove,
                        value: '=turn.todoTitle'
                    }),
                    new SendActivity('${DeleteReadBack()}'),
                    new EndDialog()
                ])
            ]
        });

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.
        this.addDialog(dialog);

        // The initial child Dialog to run.
        this.initialDialogId = DIALOG_ID;
    }

    async getToDoTitleToDelete(dc, options) {
        // Demonstrates using a custom code step to extract entities and set them in state.
        const todoList = dc.state.getValue('user.todos');
        const todoTitle = dc.state.getValue('turn.entities.todoTitle');

        // By default, recognized intents from a recognizer are available under turn.intents scope.
        // Recognized entities are available under turn.entities scope.
        if (todoTitle) {
            if (todoList.includes(todoTitle[0])) {
                // Set the todo title in turn.todoTitle scope.
                dc.state.setValue('turn.todoTitle', todoTitle[0]);
            }
        }

        return await dc.endDialog(options);
    }
}

module.exports.DeleteToDoDialog = DeleteToDoDialog;
