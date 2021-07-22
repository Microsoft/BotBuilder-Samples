// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ComponentDialog, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');

const WATERFALL_DIALOG = 'waterfallDialog';
const SSO_SIGN_IN_DIALOG = 'signInDialog';
const OAUTH_PROMPT = 'oAuthPrompt';

class SsoSignInDialog extends ComponentDialog {
    constructor(connectionName) {
        super(SSO_SIGN_IN_DIALOG);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName,
            text: 'Sign in to the root bot using Azure AD for SSO',
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
        return stepContext.beginDialog(OAUTH_PROMPT);
    }

    /**
     * Display the user's token and end the dialog.
     */
    async displayTokenStep(stepContext) {
        if (!stepContext.result.token) {
            await stepContext.context.sendActivity('No token was provided.');
        } else {
            await stepContext.context.sendActivity(`Here is your token: ${ stepContext.result.token }`);
        }

        return stepContext.endDialog();
    }
}

module.exports = {
    SsoSignInDialog,
    SSO_SIGN_IN_DIALOG
};
