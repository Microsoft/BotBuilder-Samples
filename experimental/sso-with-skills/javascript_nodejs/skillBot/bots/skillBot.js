// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler } = require('botbuilder');
const { runDialog } = require('botbuilder-dialogs');

class SkillBot extends ActivityHandler {
    constructor(conversationState, dialog) {
        super();
        if (!conversationState) throw new Error('[SkillBot]: Missing parameter. conversationState is required');
        if (!dialog) throw new Error('[SkillBot]: Missing parameter. dialog is required');

        this.conversationState = conversationState;
        this.dialog = dialog;

        this.dialogState = this.conversationState.createProperty('DialogState');

        this.onTurn(async (context, next) => {
            await runDialog(this.dialog, context, this.dialogState);
            // Ensure next BotHandler is executed.
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

module.exports.SkillBot = SkillBot;
