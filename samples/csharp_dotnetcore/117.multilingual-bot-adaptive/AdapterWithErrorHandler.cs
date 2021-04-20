// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples.Translation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, IStorage storage,
            UserState userState, ConversationState conversationState, TranslationMiddleware translationMiddleware, IConfiguration configuration)
            : base(credentialProvider)
        {
            // These methods add middleware to the adapter. The middleware adds the storage and state objects to the
            // turn context each turn so that the dialog manager can retrieve them.
            this.UseStorage(storage);
            this.UseBotState(userState);
            this.UseBotState(conversationState);

            if (translationMiddleware == null)
            {
                throw new NullReferenceException(nameof(translationMiddleware));
            }

            // Add translation middleware to the adapter's middleware pipeline
            Use(translationMiddleware);

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                await SendWithoutMiddleware(turnContext, "The bot encountered an error or bug.");
                await SendWithoutMiddleware(turnContext, "To continue to run this bot, please fix the bot source code.");

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
                        logger.LogError(e, $"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }

        private static async Task SendWithoutMiddleware(ITurnContext turnContext, string message)
        {
            // Sending the Activity directly through the Adapter rather than through the TurnContext skips the middleware processing
            // this might be important in this particular case because it might have been the TranslationMiddleware that is actually failing!
            var activity = MessageFactory.Text(message);

            // If we are skipping the TurnContext we must address the Activity manually here before sending it.
            activity.ApplyConversationReference(turnContext.Activity.GetConversationReference());

            // Send the actual Activity through the Adapter.
            await turnContext.Adapter.SendActivitiesAsync(turnContext, new[] { activity }, CancellationToken.None);
        }
    }
}
