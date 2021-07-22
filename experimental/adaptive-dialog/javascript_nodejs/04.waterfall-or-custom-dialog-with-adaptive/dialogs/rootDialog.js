// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    ComponentDialog,
    NumberPrompt,
    TextPrompt,
    WaterfallDialog,
} = require('botbuilder-dialogs');
const { AdaptiveDialog, BeginDialog, DialogExpression, EndDialog, NumberInput, OnBeginDialog, TemplateEngineLanguageGenerator } = require('botbuilder-dialogs-adaptive');

const { SlotDetails } = require('./slotDetails');
const { SlotFillingDialog } = require('./slotFillingDialog');

const ROOT_DIALOG = 'RootDialog';
const ADAPTIVE_DIALOG = 'AdaptiveDialog';

class RootDialog extends ComponentDialog {
    constructor(userState) {
        super(ROOT_DIALOG);

        this.userStateAccessor = userState.createProperty('result');

        // Rather than explicitly coding a Waterfall we have only to declare what properties we want collected.
        // In this example we will want two text prompts to run, one for the first name and one for the last.
        const fullnameSlots = [
            new SlotDetails('first', 'text', 'Please enter your first name.'),
            new SlotDetails('last', 'text', 'Please enter your last name.')
        ];

        // This defines an address dialog that collects street, city and zip properties.
        const addressSlots = [
            new SlotDetails('street', 'text', 'Please enter your street address.'),
            new SlotDetails('city', 'text', 'Please enter the city.'),
            new SlotDetails('zip', 'text', 'Please enter the zip.')
        ];

        // define adaptive dialog
        const adaptiveSlotFillingDialog = new AdaptiveDialog(ADAPTIVE_DIALOG);

        // Set a language generator
        // You can see other adaptive dialog samples to learn how to externalize generation resources into .lg files.
        adaptiveSlotFillingDialog.generator = new TemplateEngineLanguageGenerator();

        // add set of actions to perform when the adaptive dialog begins.
        adaptiveSlotFillingDialog.triggers = [
            new OnBeginDialog([
                // any options passed into adaptive dialog is automatically available under dialog.xxx
                // get user age
                new NumberInput().configure({
                    property: 'dialog.userage',
                    // use information passed in to the adaptive dialog.
                    prompt: 'Hello ${dialog.fullname.first}, what is your age?',
                    validations: [
                        'int(this.value) >= 1',
                        'int(this.value) <= 150'
                    ],
                    invalidPrompt: 'Sorry, ${this.value} does not work. Looking for age to be between 1-150. What is your age?',
                    unrecognizedPrompt: 'Sorry, I did not understand ${this.value}. What is your age?',
                    maxTurnCount: 3,
                    defaultValue: '=30',
                    defaultValueResponse: "Sorry, this is not working. For now, I'm setting your age to ${this.defaultValue}",
                    allowInterruptions: false
                }),
                new NumberInput().configure({
                    property: 'dialog.shoesize',
                    prompt: 'Please enter your shoe size.',
                    invalidPrompt: 'Sorry ${this.value} does not work. You must enter a size between 0 and 16. Half sizes are acceptable.',
                    validations: [
                        // size can only between 0-16
                        'int(this.value) >= 0 && int(this.value) <= 16',
                        // can only full or half size
                        "isMatch(string(this.value), '^[0-9]+(\\.5)*$')"
                    ],
                    allowInterruptions: false
                }),
                new BeginDialog().configure({
                    dialog: new DialogExpression('address'),
                    resultProperty: 'dialog.address'
                }),
                // return everything under dialog scope.
                new EndDialog().configure({
                    value: '=dialog'
                })
            ])
        ];

        // Add the various dialogs that will be used to the DialogSet.
        this.addDialog(new SlotFillingDialog('address', addressSlots));
        this.addDialog(new SlotFillingDialog('fullname', fullnameSlots));
        this.addDialog(new TextPrompt('text'));
        this.addDialog(new NumberPrompt('number')); // PromptCultureModels.English.locale
        this.addDialog(new NumberPrompt('shoesize', this.shoeSizeValidator)); // PromptCultureModels.English.locale

        // We will instead have adaptive dialog do the slot filling by invoking the custom dialog
        // AddDialog(new SlotFillingDialog("slot-dialog", slots));

        // Add adaptive dialog
        this.addDialog(adaptiveSlotFillingDialog);

        // Defines a simple two step Waterfall to test the slot dialog.
        this.addDialog(new WaterfallDialog('waterfall', [
            this.startDialog.bind(this),
            this.doAdaptiveDialog.bind(this),
            this.processResults.bind(this)
        ]));

        // The initial child Dialog to run.
        this.initialDialogId = 'waterfall';
    }

    async startDialog(step) {
        // Start the child dialog. This will just get the user's first and last name.
        return await step.beginDialog('fullname');
    }

    async doAdaptiveDialog(step) {
        let adaptiveOptions;
        if (step.result) {
            adaptiveOptions = { fullname: step.result.values };
        }

        // begin the adaptive dialog. This in-turn will get user's age, shoe-size using adaptive inputs and subsequently
        // call the custom slot filling dialog to fill user address.
        return await step.beginDialog(ADAPTIVE_DIALOG, adaptiveOptions);
    }

    // This is the second step of the WaterfallDialog.
    // It receives the results of the SlotFillingDialog and displays them.
    async processResults(step) {
        // Each "slot" in the SlotFillingDialog is represented by a field in step.result.values.
        // The complex that contain subfields have their own .values field containing the sub-values.
        const values = step.result;

        const fullname = values.fullname;
        await step.context.sendActivity(`Your name is ${ fullname.first } ${ fullname.last }.`);

        await step.context.sendActivity(`You wear a size ${ values.shoesize } shoes.`);

        const address = values.address.values;
        await step.context.sendActivity(`Your address is: ${ address.street }, ${ address.city } ${ address.zip }`);

        return await step.endDialog();
    }

    // Validate that the provided shoe size is between 0 and 16, and allow half steps.
    // This is used to instantiate a specialized NumberPrompt.
    async shoeSizeValidator(prompt) {
        if (prompt.recognized.succeeded) {
            const shoesize = prompt.recognized.value;

            // Shoe sizes can range from 0 to 16.
            if (shoesize >= 0 && shoesize <= 16) {
                // We only accept round numbers or half sizes.
                if (Math.floor(shoesize) === shoesize || Math.floor(shoesize * 2) === shoesize * 2) {
                    // Indicate success.
                    return true;
                }
            }
        }

        return false;
    }
}

module.exports.RootDialog = RootDialog;
