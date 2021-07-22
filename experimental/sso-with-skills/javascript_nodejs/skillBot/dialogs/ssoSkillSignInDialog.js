// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ComponentDialog, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');

const SSO_SKILL_SIGN_IN_DIALOG = 'signInDialog';
const OAUTH_PROMPT = 'oAuthPrompt';
const WATERFALL_DIALOG = 'waterfallDialog';

class SsoSkillSignInDialog extends ComponentDialog {
    constructor(connectionName) {
        super(SSO_SKILL_SIGN_IN_DIALOG);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName,
            text: 'Sign in to the Skill using Azure AD for SSO',
            title: 'Sign In'
        }))
            .addDialog(new WaterfallDialog(WATERFALL_DIALOG, [
                this.signInStep.bind(this),
                this.displayTokenStep.bind(this)
            ]));

        this.initialDialogId = WATERFALL_DIALOG;
    }

    /**
     * Display the OAuthPrompt, asking the user to log in.
     */
    async signInStep(stepContext) {
        // This prompt won't show if the user is signed in to the root using SSO.
        return stepContext.beginDialog(OAUTH_PROMPT);
    }

    /**
     * Display the user's token and end the dialog.
     */
    async displayTokenStep(stepContext) {
        if (!stepContext.result.token) {
            await stepContext.context.sendActivity('No token was provided for the skill.');
        } else {
            await stepContext.context.sendActivity(`Here is your token for the skill: ${ stepContext.result.token }`);
        }

        return stepContext.endDialog();
    }
}

module.exports = {
    SsoSkillSignInDialog,
    SSO_SKILL_SIGN_IN_DIALOG
};
