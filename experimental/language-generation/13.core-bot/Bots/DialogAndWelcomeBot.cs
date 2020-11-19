// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.LanguageGeneration;
using System;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        private Templates _templates;

        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "welcomeCard.lg" };
            string fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Actions to include in the welcome card. These are passed to LG and are then included in the generated Welcome card.
            var actions = new {
                actions = new List<Object>() {
                    new {
                        title = "Get an overview",
                        url = "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"
                    },
                    new {
                        title = "Ask a question",
                        url = "https://stackoverflow.com/questions/tagged/botframework"
                    },
                    new {
                        title = "Learn how to deploy",
                        url = "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"
                    }
                }
            };
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("WelcomeCard", actions)));
                }
            }
        }       
    }
}
