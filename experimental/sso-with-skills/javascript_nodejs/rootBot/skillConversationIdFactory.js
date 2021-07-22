// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { SkillConversationIdFactoryBase, TurnContext } = require('botbuilder');

/**
 * A SkillConversationIdFactory that uses an in memory dictionary
 * to store and retrieve ConversationReference instances.
 */
class SkillConversationIdFactory extends SkillConversationIdFactoryBase {
    constructor() {
        super();
        this.refs = {};
    }

    async createSkillConversationIdWithOptions(options) {
        // Create the SkillConversationReference instance.
        const skillConversationReference = {
            conversationReference: TurnContext.getConversationReference(options.activity),
            oAuthScope: options.fromBotOAuthScope
        };
        // This key has a 100 character limit by default. Increase with `restify.createServer({ maxParamLength: 1000 });` in index.js.
        const key = `${ options.fromBotId }-${ options.botFrameworkSkill.appId }-${ skillConversationReference.conversationReference.conversation.id }-${ skillConversationReference.conversationReference.channelId }-skillconvo`;
        // Store the skillConversationReference using the skillConversationId as a key.
        this.refs[key] = skillConversationReference;

        // Return the generated skillConversationId (that will be also used as the conversation ID to call the skill).
        return key;
    }

    async getSkillConversationReference(skillConversationId) {
        // Get the skillConversationReference from storage for the given skillConversationId.
        return this.refs[skillConversationId];
    }

    async deleteConversationReference(skillConversationId) {
        // Delete the skillConversationReference from storage.
        this.refs[skillConversationId] = undefined;
    }
}

module.exports.SkillConversationIdFactory = SkillConversationIdFactory;
