// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActionTypes, TeamsActivityHandler } = require('botbuilder');
const { CardResources } = require('../models/cardResources');
const { UIConstants } = require('../models/UIConstants');
const { TaskModuleResponseFactory } = require('../models/taskModuleResponseFactory');
const { SimpleGraphClient } = require('../simpleGraphClient');
const path = require('path');

class TeamsAdaptiveCardTabsBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.connectionName = process.env.connectionName;

        // See https://aka.ms/about-bot-activity-message to learn more about the message and other activity types.
        this.onMessage(async (context, next) => {
            if (context.activity?.text?.trim().toLowerCase() === 'logout') {
                if (typeof context.adapter.signOutUser !== 'function') {
                    throw new Error('logout: not supported by the current adapter.');
                }

                await context.adapter.signOutUser(context, this.connectionName);
                await context.sendActivity('You have been signed out.');
            } else {
                await context.sendActivity('Hello, I am a Teams Adaptive Card Tabs bot. Please use the tabs to interact. (Send "logout" to sign out)');
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    };

    async handleTeamsTabFetch(context, tabRequest) {
        if (tabRequest?.tabEntityContext?.tabEntityId === 'workday') {
            let magicCode;
            if (tabRequest.state) {
                magicCode = tabRequest.state.toString();
            }

            return this.getPrimaryTabResponse(context, magicCode);
        } else {
            return this.getTabResponse([
                CardResources.Welcome,
                CardResources.InterviewCandidates,
                CardResources.VideoId
            ]);
        }
    }

    async handleTeamsTabSubmit(context, tabSubmit) {
        if (tabSubmit?.tabEntityContext?.tabEntityId === 'workday') {
            if (context.activity?.value?.data?.shouldLogout) {
                const credentials = this.getCredentials(context);

                await context.adapter.signOutUser(context, credentials, this.connectionName);
                return this.getTabResponse([CardResources.Success]);
            }

            const response = await this.getPrimaryTabResponse(context, null);
            // If the user is not signed in, the .Tab type will be auth.
            const successCard = {
                card: this.createAdaptiveCardAttachment(CardResources.Success)
            };
            response.tab.value.cards.splice(0, 0, successCard);

            return response;
        } else {
            return this.getTabResponse([
                CardResources.Welcome,
                CardResources.InterviewCandidates,
                CardResources.VideoId
            ]);
        }
    }

    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const videoId = taskModuleRequest?.data?.youTubeVideoId?.toString();
        const taskInfo = {
            height: UIConstants.YouTube.Height,
            width: UIConstants.YouTube.Width,
            title: UIConstants.YouTube.Title.toString()
        };
        if (videoId) {
            taskInfo.url = taskInfo.fallbackUrl = `https://www.youtube.com/embed/${ videoId }`;
        } else {
            // No video ID is present, so return the InputText card.
            const attachment = {
                contentType: 'application/vnd.microsoft.card.adaptive',
                content: this.createAdaptiveCardAttachment(CardResources.InputText)
            };
            taskInfo.card = attachment;
        }

        return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    }

    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        return {
            task: {
                type: 'message',
                value: 'Thanks!'
            }
        };
    }

    getCredentials(context) {
        const credentials = context.turnState.get(context.adapter?.ConnectorClientKey)?.credentials;
        if (!credentials) {
            throw new Error('The current adapter does not support OAuth.');
        }

        return credentials;
    }

    async getPrimaryTabResponse(context, magicCode) {
        const credentials = this.getCredentials(context);
        const token = await context.adapter.getUserToken(context, credentials, this.connectionName, magicCode);
        // If a token is returned, the user is logged in.
        if (!token?.token) {
            const cards = [
                CardResources.QuickActions,
                CardResources.AdminConfig
            ].map((card) => {
                return this.createAdaptiveCardAttachment(card);
            });

            // Add the manager card.
            const user = await new SimpleGraphClient(token.token).getMe();
            const managerCard = {
                card: this.createAdaptiveCardAttachment(CardResources.ManagerDashboard, '{profileName}', user?.displayName ?? '[unknown]')

            };
            cards.splice(1, 0, managerCard);

            return {
                tab: {
                    type: 'continue',
                    value: {
                        cards
                    }
                }
            };
        }

        // The user is not logged in, so send an 'auth' response.
        const signInResource = await context.adapter.getSignInResource(context, credentials, this.connectionName, context.activity.from.id);
        return {
            tab: {
                type: 'auth',
                suggestedActions: {
                    actions: [
                        {
                            title: 'login',
                            type: ActionTypes.OpenUrl,
                            value: signInResource.signInLink
                        }
                    ]
                }
            }
        };
    }

    getTabResponse(cardTypes) {
        const cards = cardTypes.map((card) => {
            return {
                card: this.createAdaptiveCardAttachment(card)
            };
        });

        return {
            tab: {
                type: 'continue',
                value: {
                    cards
                }
            }
        };
    }

    createAdaptiveCardAttachment(card, replaceText, replacement) {
        const cardResourcePath = path.join(__dirname, '..', 'resources', card);
        const cardJson = require(cardResourcePath);
        if (replaceText && replacement) {
            cardJson.replace(replaceText, replacement);
        }

        return JSON.parse(cardJson);
    }
}

module.exports.TeamsAdaptiveCardTabsBot = TeamsAdaptiveCardTabsBot;
