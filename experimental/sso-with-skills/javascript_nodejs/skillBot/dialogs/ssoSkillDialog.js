// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { MessageFactory, InputHints } = require('botbuilder');
const { ComponentDialog, ChoicePrompt, WaterfallDialog, ChoiceFactory, DialogTurnStatus } = require('botbuilder-dialogs');
const { SsoSkillSignInDialog, SSO_SKILL_SIGN_IN_DIALOG } = require('./ssoSkillSignInDialog');

const SSO_SKILL_DIALOG = 'ssoSkillDialog';
const ACTION_STEP_PROMPT = 'actionStepPrompt';
const WATERFALL_DIALOG = 'waterfallDialog';

class SsoSkillDialog extends ComponentDialog {
    constructor() {
        super(SSO_SKILL_DIALOG);

        this.connectionName = process.env.connectionName;
        if (!this.connectionName) {
            throw new Error('"ConnectionName" is not set in configuration.');
        }

        this.addDialog(new SsoSkillSignInDialog(this.connectionName))
            .addDialog(new ChoicePrompt(ACTION_STEP_PROMPT));

        const waterfallSteps = [
            this.promptActionStep.bind(this),
            this.handleActionStep.bind(this),
            this.promptFinalStep.bind(this)
        ];

        this.addDialog(new WaterfallDialog(WATERFALL_DIALOG, waterfallSteps));

        this.initialDialogId = WATERFALL_DIALOG;
    }

    async promptActionStep(stepContext) {
        const messageText = 'What SSO action do you want to perform on the skill?';
        const repromptMessageText = 'That was not a valid choice, please select a valid choice.';
        const options = {
            prompt: MessageFactory.text(messageText, messageText, InputHints.ExpectingInput),
            retryPrompt: MessageFactory.text(repromptMessageText, repromptMessageText, InputHints.ExpectingInput),
            choices: await this.getPromptChoices(stepContext)
        };

        // Prompt the user to select a skill.
        return stepContext.prompt(ACTION_STEP_PROMPT, options);
    }

    async getPromptChoices(stepContext) {
        const promptChoices = [];
        const adapter = stepContext.context.adapter;

        // Show different options if the user is signed in on the parent or not.
        const token = await adapter.getUserToken(stepContext.context, this.connectionName, null);
        if (!token) {
            promptChoices.push(
                'Login to the skill'
            );
        } else {
            promptChoices.push(
                'Logout from the skill',
                'Show token'
            );
        }

        promptChoices.push('End');

        return ChoiceFactory.toChoices(promptChoices);
    }

    async handleActionStep(stepContext) {
        const action = stepContext.result.value.toLowerCase();

        const adapter = stepContext.context.adapter;

        switch (action) {
            case 'login to the skill':
                // The SsoSkillSignInDialog will just show the user token if the user logged on to the root bot.
                return stepContext.beginDialog(SSO_SKILL_SIGN_IN_DIALOG);
            case 'logout from the skill':
                // This will just clear the token from the skill.
                await adapter.signOutUser(stepContext.context, this.connectionName);
                await stepContext.context.sendActivity('You have been signed out.');
                return stepContext.next();

            case 'show token':
                var token = await adapter.getUserToken(stepContext.context, this.connectionName);
                if (!token) {
                    await stepContext.context.sendActivity('User has no cached token.');
                } else {
                    await stepContext.context.sendActivity(`Here is your current SSO token for the skill: ${ token.token }`);
                }

                return stepContext.next();

            case 'end':
                return { status: DialogTurnStatus.complete };

            default:
                // This should never be hit since the previous prompt validates the choice.
                throw new Error(`Unrecognized action: ${ action }`);
        }
    }

    async promptFinalStep(stepContext) {
        // Restart the dialog (we will exit when the user says "end").
        return stepContext.replaceDialog(this.initialDialogId);
    }
}

module.exports = {
    SsoSkillDialog,
    SSO_SKILL_DIALOG
};
