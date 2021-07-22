// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler } = require('botbuilder');
const { DialogManager } = require('botbuilder-dialogs');

/**
 * This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
 * to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
 * each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
 * The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
 * and the requirement is that all BotState objects are saved at the end of a turn.
 */
class DialogBot extends ActivityHandler {
    /**
     *
     * @param {Dialog} dialog
     */
    constructor(dialog) {
        super();
        if (!dialog) throw new Error('[DialogBot]: Missing parameter. dialog is required');

        this.dialogManager = new DialogManager(dialog);

        this.onTurn(async (context, next) => {
            console.log('Running dialog with activity.');

            await this.dialogManager.onTurn(context);

            await next();
        });
    }
}

module.exports.DialogBot = DialogBot;
