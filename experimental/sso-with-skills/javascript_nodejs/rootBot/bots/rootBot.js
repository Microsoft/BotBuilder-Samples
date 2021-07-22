// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, ActivityTypes } = require('botbuilder');
const { runDialog } = require('botbuilder-dialogs');

class RootBot extends ActivityHandler {
    constructor(conversationState, dialog) {
        super();
        if (!conversationState) throw new Error('[RootBot]: Missing parameter. conversationState is required');
        if (!dialog) throw new Error('[RootBot]: Missing parameter. dialog is required');

        this.conversationState = conversationState;
        this.dialogState = this.conversationState.createProperty('DialogState');
        this.mainDialog = dialog;

        this.botId = process.env.MicrosoftAppId;
        if (!this.botId) {
            throw new Error('[RootBot] MicrosoftAppId is not set in configuration');
        }

        this.onTurn(async (context, next) => {
            if (context.activity.type !== ActivityTypes.ConversationUpdate) {
                // Forward the activity to the dialog.
                await runDialog(this.mainDialog, context, this.dialogState);
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let cnt = 0; cnt < membersAdded.length; cnt++) {
                // Greet anyone that was not the target (recipient) of this message.
                if (membersAdded[cnt].id !== context.activity.recipient.id) {
                    await context.sendActivity('Hello and welcome!');
                    await runDialog(this.mainDialog, context, this.dialogState);
                }
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    /**
     * Override the ActivityHandler.run() method to save state changes after the bot logic completes.
     */
    async run(context) {
        await super.run(context);

        // Save any state changes. The load happened during the execution of the dialog.
        await this.conversationState.saveChanges(context, false);
    }
}

module.exports.RootBot = RootBot;
