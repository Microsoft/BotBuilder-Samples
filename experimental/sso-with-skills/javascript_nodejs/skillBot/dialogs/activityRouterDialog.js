// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes, MessageFactory, InputHints } = require('botbuilder');
const { ComponentDialog, WaterfallDialog, DialogTurnStatus } = require('botbuilder-dialogs');
const { SsoSkillDialog, SSO_SKILL_DIALOG } = require('./ssoSkillDialog');

const ACTIVITY_ROUTER_DIALOG = 'activityRouterDialog';
const WATERFALL_DIALOG = 'waterfallDialog';

class ActivityRouterDialog extends ComponentDialog {
    constructor() {
        super(ACTIVITY_ROUTER_DIALOG);

        this.addDialog(new SsoSkillDialog())
            .addDialog(new WaterfallDialog(WATERFALL_DIALOG, [
                this.processActivity.bind(this)
            ]));

        // The initial child dialog to run.
        this.initialDialogId = WATERFALL_DIALOG;
    }

    async processActivity(stepContext) {
        // A skill can send trace activities, if needed.
        await stepContext.context.sendTraceActivity('ActivityRouterDialog.processActivity', `Got ActivityType: ${ stepContext.context.activity.type }`);

        // In this simple skill, we only handle SSO events.
        if (stepContext.context.activity.type === ActivityTypes.Event && stepContext.context.activity.name === 'Sso') {
            return stepContext.beginDialog(SSO_SKILL_DIALOG);
        }

        // We didn't get an activity type we can handle.
        const message = `UnrecognizedActivityType: "${ stepContext.context.activity.type }".`;
        await stepContext.context.sendActivity(MessageFactory.text(message, message, InputHints.IgnoringInput));
        return { status: DialogTurnStatus.complete };
    }
}

module.exports = {
    ActivityRouterDialog,
    ACTIVITY_ROUTER_DIALOG
};
