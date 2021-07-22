// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, ArrayChangeType, ConfirmInput, EditArray, EndDialog, IfCondition, OnBeginDialog, OnIntent, RegexRecognizer, SendActivity, SetProperty, TextInput, TemplateEngineLanguageGenerator } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const path = require('path');

const DIALOG_ID = 'ADD_TO_DO_DIALOG';

class AddToDoDialog extends ComponentDialog {
    constructor() {
        super(DIALOG_ID);
        const lgFile = Templates.parseFile(path.join(__dirname, 'addToDoDialog.lg'));
        // Create instance of adaptive dialog.
        const dialog = new AdaptiveDialog(DIALOG_ID).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            // Create and use a regex recognizer on the child.
            // Each child adaptive dialog can have its own recognizer.
            // This sample demonstrates use of a regex recognizer in a child dialog.
            recognizer: this.createRegExRecognizer(),
            triggers: [
                new OnBeginDialog([
                    // Take todo title if we already have it from root dialog's LUIS model.
                    // This is the title entity defined in ../rootDialog/rootDialog.lu.
                    // There is one LUIS application for this bot. So any entity captured by the rootDialog
                    // will be automatically available to child dialog.
                    // @EntityName is a short-hand for turn.entities.<EntityName>. Other useful short-hands are
                    //     #IntentName is a short-hand for turn.intents.<IntentName>
                    //     $PropertyName is a short-hand for dialog.<PropertyName>
                    new SetProperty().configure({
                        property: 'turn.todoTitle',
                        value: '=@todoTitle'
                    }),
                    // TextInput by default will skip the prompt if the property has value.
                    new TextInput().configure({
                        property: 'turn.todoTitle',
                        prompt: '${GetToDoTitle()}'
                    }),
                    // Add the new todo title to the list of todos. Keep the list of todos in the user scope.
                    new EditArray().configure({
                        itemsProperty: 'user.todos',
                        changeType: ArrayChangeType.push,
                        value: '=turn.todoTitle'
                    }),
                    new SendActivity('${AddToDoReadBack()}')
                    // All child dialogs will automatically end if there are no additional steps to execute.
                    // If you wish for a child dialog to not end automatically, you can set
                    // AutoEndDialog property on the Adaptive Dialog to 'false'.
                ]),
                new OnIntent('None', [], [
                    new SetProperty().configure({
                        property: 'turn.todoTitle',
                        value: '=turn.activity.text'
                    })
                ]),
                new OnIntent('Help', [], [new SendActivity('${HelpAddToDo()}')]),
                new OnIntent('Cancel', [], [
                    new ConfirmInput().configure({
                        property: 'turn.addTodo.cancelConfirmation',
                        prompt: '${ConfirmCancellation()}',
                        // Allow interruptions is an expression. So you can write any expression to determine if an interruption should be allowed.
                        // In this case, we will disallow interruptions since this is a cancellation confirmation.
                        allowInterruptions: false,
                        // Controls the number of times user is prompted for this input.
                        maxTurnCount: 1,
                        // Default value to use if we have hit the MaxTurnCount
                        defaultValue: '=false',
                        // You can refer to properties of this input via %propertyName notation.
                        // The default response is sent if we have prompted the user for MaxTurnCount number of times
                        // and if a default value is assumed for the property.
                        defaultValueResponse: "Sorry, I do not recognize '${this.value}'. I'm going with '${%DefaultValue}' for now to be safe."
                    }),
                    new IfCondition().configure({
                        // All conditions are expressed using the common expression language.
                        // See https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/common-expression-language to learn more
                        condition: 'turn.addTodo.cancelConfirmation == true',
                        actions: [
                            new SendActivity('${CancelAddTodo()}'),
                            new EndDialog()
                        ],
                        elseActions: [new SendActivity("${HelpPrefix()}, let's get right back to adding a todo.")]
                        // We do not need to specify an else block here since if user said no,
                        // the control flow will automatically return to the last active step (if any)
                    })
                ])
            ]
        });
        this.addDialog(dialog);
        this.initialDialogId = DIALOG_ID;
    }

    createRegExRecognizer() {
        return new RegexRecognizer().configure({
            intents: [
                { intent: 'Help', pattern: '(?i)help' },
                { intent: 'Cancel', pattern: '(?i)cancel|never mind' }
            ]
        });
    }
}

module.exports.AddToDoDialog = AddToDoDialog;
