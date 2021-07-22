// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ComponentDialog } = require('botbuilder-dialogs');
const { AdaptiveDialog, AttachmentInput, ConfirmInput, IfCondition, NumberInput, OnBeginDialog, SendActivity, TemplateEngineLanguageGenerator, TextInput } = require('botbuilder-dialogs-adaptive');
const { Templates } = require('botbuilder-lg');

const path = require('path');

const ROOT_DIALOG = 'USER_PROFILE_DIALOG';

class UserProfileDialog extends ComponentDialog {
    constructor() {
        super('userProfileDialog');
        const lgFile = Templates.parseFile(path.join(__dirname, 'userProfileDialog.lg'));
        const userProfileAdaptiveDialog = new AdaptiveDialog(ROOT_DIALOG).configure({
            generator: new TemplateEngineLanguageGenerator(lgFile),
            triggers: [
                new OnBeginDialog([
                    // Ask for user's age and set it in user.userProfile scope.
                    new TextInput().configure(
                        {
                            // Set the output of the text input to this property in memory.
                            property: 'user.userProfile.Transport',
                            prompt: '${ModeOfTransportPrompt()}'
                        }),
                    new TextInput().configure(
                        {
                            property: 'user.userProfile.Name',
                            prompt: '${AskForName()}'
                        }),
                    // SendActivity supports full language generation resolution.
                    // See here to learn more about language generation
                    // https://aka.ms/language-generation
                    new SendActivity('${AckName()}'),
                    new ConfirmInput().configure(
                        {
                            property: 'turn.ageConfirmation',
                            prompt: '${AgeConfirmPrompt()}'
                        }),
                    new IfCondition().configure(
                        {
                            // All conditions are expressed using the common expression language.
                            // See https://aka.ms/adaptive-expressions to learn more
                            condition: 'turn.ageConfirmation == true',
                            actions: [
                                new NumberInput().configure(
                                    {
                                        prompt: '${AskForAge()}',
                                        property: 'user.userProfile.Age',
                                        // Add validations
                                        validations: [
                                            // Age must be greater than or equal 1
                                            'int(this.value) >= 1',
                                            // Age must be less than 150
                                            'int(this.value) < 150'
                                        ],
                                        invalidPrompt: '${AskForAge.invalid()}',
                                        unrecognizedPrompt: '${AskForAge.unRecognized()}'
                                    }),
                                new SendActivity('${UserAgeReadBack()}')
                            ],
                            elseActions: [
                                new SendActivity('${NoAge()}')
                            ]
                        }),
                    new IfCondition().configure(
                        {
                            condition: 'turn.activity.channelId == "msteams"',
                            actions: [
                                // This attachment prompt example is not designed to work for Teams attachments, so skip it in this case
                                new SendActivity('Skipping attachment prompt in Teams channel...')
                            ],
                            elseActions: [
                                new AttachmentInput().configure(
                                    {
                                        prompt: '${AskForImage()}',
                                        property: 'user.userProfile.picture',
                                        validations: [
                                            'this.value.contentType == "image/jpeg" || this.value.contentType == "image/png"'
                                        ],
                                        invalidPrompt: '${AskForImage.Invalid()}'
                                    })
                            ]
                        }),
                    new ConfirmInput().configure(
                        {
                            prompt: '${ConfirmPrompt()}',
                            property: 'turn.finalConfirmation'
                        }),
                    // Use LG template to come back with the final read out.
                    // This LG template is a great example of what logic can be wrapped up in LG sub-system.
                    new SendActivity('${FinalUserProfileReadOut()}') // examines turn.finalConfirmation
                ])
            ]
        });
        this.addDialog(userProfileAdaptiveDialog);
        this.initialDialogId = ROOT_DIALOG;
    }
}

module.exports.UserProfileDialog = UserProfileDialog;
