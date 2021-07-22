// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    ComponentDialog,
    NumberPrompt,
    TextPrompt,
    WaterfallDialog
} = require('botbuilder-dialogs');
const { SlotDetails } = require('./slotDetails');
const { SlotFillingDialog } = require('./slotFillingDialog');

const ROOT_DIALOG = 'RootDialog';

class RootDialog extends ComponentDialog {
    constructor(resourceExplorer) {
        super(ROOT_DIALOG);

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

        // Dialogs can be nested and the slot filling dialog makes use of that. In this example some of the child
        // dialogs are slot filling dialogs themselves.
        const slots = [
            new SlotDetails('address', 'address')
        ];

        // Add the various dialogs that will be used to the DialogSet.
        this.addDialog(new SlotFillingDialog('address', addressSlots));
        this.addDialog(new SlotFillingDialog('fullname', fullnameSlots));
        this.addDialog(new TextPrompt('text'));
        this.addDialog(new NumberPrompt('number')); // PromptCultureModels.English.locale
        this.addDialog(new NumberPrompt('shoesize', this.shoeSizeValidator)); // PromptCultureModels.English.locale
        this.addDialog(new SlotFillingDialog('slot-dialog', slots));

        // Defines a simple two step Waterfall to test the slot dialog.
        this.addDialog(new WaterfallDialog('waterfall', [
            this.startDialog.bind(this),
            this.doComposerDialog.bind(this),
            this.doSlotDialog.bind(this),
            this.processResults.bind(this)
        ]));

        // Load and add adaptive dialog produced by composer.
        // Name of the dialog (.dialog file name) to find
        const dialogResource = resourceExplorer.getResource('main-without-luis.dialog');
        const composerDialog = resourceExplorer.loadType(dialogResource);
        // give the dialog an ID, this defaults to the filename if missing.
        composerDialog.id = 'adaptive-main';
        // Add the dialog
        this.addDialog(composerDialog);

        // The initial child Dialog to run.
        this.initialDialogId = 'waterfall';
    }

    async startDialog(step) {
        // Start the child dialog. This will just get the user's first and last name.
        return await step.beginDialog('fullname');
    }

    async doComposerDialog(step) {
        let adaptiveOptions;
        if (step.result) {
            adaptiveOptions = { fullname: step.result.values };
        }

        // begin the adaptive dialog. This in-turn will get user's age, shoe-size using adaptive inputs and subsequently
        // call the custom slot filling dialog to fill user address.
        return await step.beginDialog('adaptive-main', adaptiveOptions);
    }

    async doSlotDialog(step) {
        let slotFillingOptions;
        if (step.result) {
            slotFillingOptions = {
                fullname: step.result.fullname,
                shoesize: step.result.shoesize,
                userage: step.result.userage
            };
        }
        return await step.beginDialog('slot-dialog', slotFillingOptions);
    }

    // This is the second step of the WaterfallDialog.
    // It receives the results of the SlotFillingDialog and displays them.
    async processResults(step) {
        // Each "slot" in the SlotFillingDialog is represented by a field in step.result.values.
        // The complex that contain subfields have their own .values field containing the sub-values.
        const values = step.result.values;

        const fullname = values.fullname;
        await step.context.sendActivity(`Your name is ${ fullname.first } ${ fullname.last }.`);

        await step.context.sendActivity(`You wear a size ${ values.shoeSize } shoes.`);

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
