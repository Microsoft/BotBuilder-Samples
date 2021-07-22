// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { LuisAdaptiveRecognizer } = require('botbuilder-ai');
const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, IfCondition, OnBeginDialog, OnIntent, SendActivity, SetProperty, TemplateEngineLanguageGenerator, TextInput} = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const path = require('path');

const DIALOG_ID = 'GET_USER_PROFILE_DIALOG';

class GetUserProfileDialog extends ComponentDialog {
    constructor() {
        super(DIALOG_ID);
        const lgFile = Templates.parseFile(path.join(__dirname, 'getUserProfileDialog.lg'));
        const dialog = new AdaptiveDialog(DIALOG_ID).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            recognizer: this.createLuisRecognizer(),
            triggers: [
                // Actions to execute when this dialog begins. This dialog will attempt to fill user profile.
                // Each adaptive dialog can have its own recognizer. The LU definition for this dialog is under GetUserProfileDialog.lu
                // This dialog supports local intents. When you say things like 'why do you need my name' or 'I will not give you my name'
                // it uses its own recognizer to handle those.
                // It also demonstrates the consultation capability of adaptive dialog. When the local recognizer does not come back with
                // a high-confidence recognition, this dialog will defer to the parent dialog to see if it wants to handle the user input.
                new OnBeginDialog([
                    new SetProperty().configure({
                        property: "user.profile.name",
                        
                        // Whenever an adaptive dialog begins, any options passed in to the dialog via 'BeginDialog' are available through dialog.xxx scope.
                        // Coalesce is a prebuilt function available as part of Adaptive Expressions. Take the first non-null value.
                        // @EntityName is a short-hand for turn.recognized.entities.<EntityName>. Other useful short-hands are 
                        //     #IntentName is a short-hand for turn.intents.<IntentName>
                        //     $PropertyName is a short-hand for dialog.<PropertyName>
                        value: "=coalesce(dialog.userName, @personName)"
                    }),
                    new SetProperty().configure(
                    {
                        property: "user.profile.age",
                        value: "=coalesce(dialog.userAge, @age)"
                    }),
                    new TextInput().configure({
                        property: "user.profile.name",
                        prompt: "${AskFirstName()}",
                        validations: [
                            // Name must be 3-50 characters in length.
                            // Validations are expressed using Adaptive expressions
                            // You can access the current candidate value for this input via 'this.value'
                            "count(this.value) >= 3",
                            "count(this.value) <= 50"
                        ],
                        invalidPrompt: "${AskFirstName.Invalid()}",
                        
                        // Because we have a local recognizer, we can use it to extract entities.
                        // This enables users to say things like 'my name is vishwac' and we only take 'vishwac' as the name.
                        value: "=@personName",
                        
                        // We are going to allow any interruption for a high confidence interruption intent classification .or.
                        // when we do not get a value for the personName entity. 
                        allowInterruptions: "turn.recognized.score >= 0.9 || !@personName"
                    }),
                    new TextInput().configure(
                    {
                        property: "user.profile.age",
                        prompt: "${AskUserAage()}",
                        validations: [
                            // Age must be within 1-150.
                            "int(this.value) >= 1",
                            "int(this.value) <= 150"
                        ],
                        invalidPrompt: "${AskUserAge.Invalid()}",
                        unrecognizedPrompt: "${AskUserAge.Unrecognized()}",

                        // We have both a number recognizer as well as age prebuilt entity recognizer added. So take either one we get.
                        // LUIS returns a complex type for prebuilt age entity. Take just the number value.
                        value: "=coalesce(@age.number, @number)",

                        // Allow interruption if we do not get either an age or a number.
                        allowInterruptions: "!@age && !@number"
                    }),
                    new SendActivity("${ProfileReadBack()}")
                ]),
                new OnIntent("Why", [], [
                        new SendActivity("${WhyJustificationReadBack()}")
                ], "#Why.Score >= 0.9"),
                new OnIntent("NoValue", [], [
                    new IfCondition().configure(
                    {
                        condition: "user.profile.name == null",
                        actions: [
                            new SetProperty().configure(
                            {
                                property: "user.profile.name",
                                value: "Human"
                            }),
                            new SendActivity("${NoValueForUserNameReadBack()}")
                        ],
                        elseActions: [
                            new SetProperty().configure(
                            {
                                property: "user.profile.age",
                                value: "30"
                            }),
                            new SendActivity("${NoValueForUserAgeReadBack()}")
                        ]
                    })
                ], "#NoValue.Score >= 0.9"),
            ]
        });

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.
        this.addDialog(dialog);

        // The initial child Dialog to run.
        this.initialDialogId = DIALOG_ID;
    }

    createLuisRecognizer() {
        if (process.env.getUserProfileDialog_en_us_lu === "" || process.env.LuisAPIHostName === "" || process.env.LuisAPIKey === "")
            throw `Sorry, you need to configure your LUIS application and update .env file.`;
        return new LuisAdaptiveRecognizer().configure({
            endpoint: process.env.LuisAPIHostName,
            endpointKey: process.env.LuisAPIKey,
            applicationId: process.env.getUserProfileDialog_en_us_lu
        });
    }
}

module.exports.GetUserProfileDialog = GetUserProfileDialog;
