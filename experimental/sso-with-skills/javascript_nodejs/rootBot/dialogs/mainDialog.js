// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes, MessageFactory, InputHints } = require('botbuilder');
const { ComponentDialog, SkillDialog, ChoicePrompt, WaterfallDialog, ChoiceFactory } = require('botbuilder-dialogs');
const { SSO_SIGN_IN_DIALOG, SsoSignInDialog } = require('./ssoSignInDialog');

const MAIN_DIALOG = 'mainDialog';
const WATERFALL_DIALOG = 'waterfallDialog';
const SKILL_DIALOG = 'skillDialog';
const ACTION_STEP_PROMPT = 'actionStepPrompt';

class MainDialog extends ComponentDialog {
    constructor(conversationState, skillsConfig, skillClient, conversationIdFactory) {
        super(MAIN_DIALOG);

        const botId = process.env.MicrosoftAppId;
        if (!botId) {
            throw new Error('MicrosoftAppId is not set in configuration.');
        }

        this.connectionName = process.env.ConnectionName;
        if (!this.connectionName) {
            throw new Error('"ConnectionName" is not set in configuration.');
        }

        // We use a single skill in this sample.
        const targetSkillId = 'SkillBot';
        if (!skillsConfig.skills[targetSkillId]) {
            throw new Error(`Skill with ID "${ targetSkillId }" not found in configuration`);
        } else {
            this.ssoSkill = skillsConfig.skills[targetSkillId];
        }

        const waterfallSteps = [
            this.promptActionStep.bind(this),
            this.handleActionStep.bind(this),
            this.promptFinalStep.bind(this)
        ];

        this.addDialog(new ChoicePrompt(ACTION_STEP_PROMPT))
            .addDialog(new SsoSignInDialog(this.connectionName))
            .addDialog(new SkillDialog(this.createSkillDialogOptions(skillsConfig, botId, conversationIdFactory, conversationState, skillClient), SKILL_DIALOG))
            .addDialog(new WaterfallDialog(WATERFALL_DIALOG, waterfallSteps));

        const activeSkillPropertyName = `${ MAIN_DIALOG }.activeSkillProperty`;
        this.activeSkillProperty = conversationState.createProperty(activeSkillPropertyName);

        this.initialDialogId = WATERFALL_DIALOG;
    }

    createSkillDialogOptions(skillsConfig, botId, conversationIdFactory, conversationState, skillClient) {
        return {
            botId,
            conversationIdFactory,
            conversationState,
            skillClient,
            skillHostEndpoint: skillsConfig.skillHostEndpoint,
            skill: this.ssoSkill
        };
    }

    async promptActionStep(stepContext) {
        const messageText = 'What SSO action do you want to perform?';
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
        let promptChoices;
        const adapter = stepContext.context.adapter;

        // Show different options if the user is signed in on the parent or not.
        const token = await adapter.getUserToken(stepContext.context, this.connectionName, null);
        if (!token) {
            promptChoices = ChoiceFactory.toChoices([
                'Login to the root bot',
                'Call Skill (without SSO)'
            ]);
        } else {
            promptChoices = ChoiceFactory.toChoices([
                'Logout from the root bot',
                'Show token',
                'Call Skill (with SSO)'
            ]);
        }

        return promptChoices;
    }

    async handleActionStep(stepContext) {
        const action = stepContext.result.value.toLowerCase();

        const adapter = stepContext.context.adapter;

        switch (action) {
            case 'login to the root bot':
                return stepContext.beginDialog(SSO_SIGN_IN_DIALOG);

            case 'logout from the root bot':
                await adapter.signOutUser(stepContext.context, this.connectionName);
                await stepContext.context.sendActivity('You have been signed out.');
                return stepContext.next();

            case 'show token':
                var token = await adapter.getUserToken(stepContext.context, this.connectionName);
                if (!token) {
                    await stepContext.context.sendActivity('User has no cached token.');
                } else {
                    await stepContext.context.sendActivity(`Here is your current SSO token: ${ token.token }`);
                }

                return stepContext.next();

            case 'call skill (with sso)':
            case 'call skill (without sso)':
                var beginSkillActivity = {
                    type: ActivityTypes.Event,
                    name: 'Sso'
                };

                // Save active skill in state (this is use in case of errors in the AdapterWithErrorHandler).
                await this.activeSkillProperty.set(stepContext.context, this.ssoSkill);

                return stepContext.beginDialog(SKILL_DIALOG, { activity: beginSkillActivity });

            default:
                throw new Error(`Unrecognized action: ${ action }`);
        }
    }

    async promptFinalStep(stepContext) {
        // Clear active skill in state.
        await this.activeSkillProperty.delete(stepContext.context);

        // Restart the dialog (we will exit when the user says end).
        return stepContext.replaceDialog(this.initialDialogId);
    }
}

module.exports.MainDialog = MainDialog;
