// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityEx, CardFactory, SkillHandler, TurnContext } = require('botbuilder');
const { JwtTokenValidation } = require('botframework-connector');
const { ActivityTypes } = require('botframework-schema');
const { v4 } = require('uuid');

class TokenExchangeSkillHandler extends SkillHandler {
    constructor(
        adapter,
        bot,
        conversationIdFactory,
        skillsConfig,
        skillClient,
        credentialProvider,
        authConfig,
        channelService = null
    ) {
        super(adapter, bot, conversationIdFactory, credentialProvider, authConfig, channelService);

        if (!adapter) throw new Error('adapter is required for TokenExchangeSkillHandler');
        if (!skillsConfig) throw new Error('skillsConfig is required for TokenExchangeSkillHandler');
        if (!skillClient) throw new Error('skillClient is required for TokenExchangeSkillHandler');
        if (!conversationIdFactory) throw new Error('conversationIdFactory is required for TokenExchangeSkillHandler');

        this.adapter = adapter;
        this.skillsConfig = skillsConfig;
        this.skillClient = skillClient;
        this.conversationIdFactory = conversationIdFactory;

        this.botId = process.env.MicrosoftAppId;
        this.connectionName = process.env.ConnectionName;
    }

    async onSendToConversation(claimsIdentity, conversationId, activity) {
        if (await this.interceptOAuthCards(claimsIdentity, activity)) {
            return {
                id: v4()
            };
        }

        return await super.onSendToConversation(claimsIdentity, conversationId, activity);
    }

    async onReplyToActivity(claimsIdentity, conversationId, activityId, activity) {
        if (await this.interceptOAuthCards(claimsIdentity, activity)) {
            return {
                id: v4()
            };
        }

        return await super.onReplyToActivity(claimsIdentity, conversationId, activityId, activity);
    }

    getCallingSkill(claimsIdentity) {
        const appId = JwtTokenValidation.getAppIdFromClaims(claimsIdentity.claims);

        if (!appId) {
            return null;
        }

        return Object.values(this.skillsConfig.skills).find((s) => s.appId === appId);
    }

    async interceptOAuthCards(claimsIdentity, activity) {
        const oathCardAttachment = activity.attachments?.find((a) => a.contentType === CardFactory.contentTypes.oauthCard);
        if (oathCardAttachment) {
            const targetSkill = this.getCallingSkill(claimsIdentity);
            if (targetSkill) {
                const oauthCard = oathCardAttachment.content;

                if (oauthCard?.tokenExchangeResource?.uri) {
                    const context = new TurnContext(this.adapter, activity);

                    context.turnState.set('BotIdentity', claimsIdentity);

                    // Azure AD Token Exchange
                    try {
                        const result = await this.adapter.exchangeToken(
                            context,
                            this.connectionName,
                            activity.recipient.id,
                            { uri: oauthCard.tokenExchangeResource.uri }
                        );

                        if (result?.token) {
                            // If token above is null, then SSO has failed and hence we return false.
                            // If not, send an invoke to the skill with the token.
                            return this.sendTokenExchangeInvokeToSkill(activity, oauthCard.tokenExchangeResource.uri, result.token, oauthCard.connectionName, targetSkill);
                        }
                    } catch (err) {
                        // This will show oauth card if token exchange fails.
                        // A common cause for hitting this section of the code is when then user hasn't provided consent to the skill app.
                        console.warn(`Unable to get SSO token for OAuthCard, exception was ${ err }`);
                        return false;
                    }
                }
            }
        }

        return false;
    }

    async sendTokenExchangeInvokeToSkill(incomingActivity, id, token, connectionName, targetSkill) {
        const activity = ActivityEx.createReply(incomingActivity);
        activity.type = ActivityTypes.Invoke;
        activity.name = 'signin/tokenExchange';
        activity.value = {
            id,
            token,
            connectionName
        };

        const skillConversationReference = await this.conversationIdFactory.getSkillConversationReference(incomingActivity.conversation.id);
        activity.conversation = skillConversationReference.conversationReference.conversation;
        activity.serviceUrl = skillConversationReference.conversationReference.serviceUrl;

        // Route the activity to the skill.
        const response = await this.skillClient.postToSkill(this.botId, targetSkill, this.skillsConfig.skillHostEndpoint, activity);

        // Check response status: true if success, false if failure.
        return (response.status >= 200 && response.status <= 299);
    }
}

module.exports.TokenExchangeSkillHandler = TokenExchangeSkillHandler;
