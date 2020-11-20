// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private Templates _templates;
        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, IStorage storage,
            UserState userState, ConversationState conversationState, IConfiguration configuration)
            : base(credentialProvider)
        {
            this.UseStorage(storage);
            this.UseBotState(userState);
            this.UseBotState(conversationState);

            string[] paths = { ".", $"{nameof(AdapterWithErrorHandler)}.lg" };
            string fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("SomethingWentWrong", exception)));

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}
