// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class CardBot : ActivityHandler
    {
        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.SendActivityAsync(MessageFactory.Attachment(new CardResource(Path.Combine(".", "Cards", "CardOne.json")).AsAttachment()));

            return Task.CompletedTask;
        }

        protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.SendActivityAsync(MessageFactory.Attachment(new CardResource(Path.Combine(".", "Cards", "CardOne.json")).AsAttachment()));

            return Task.CompletedTask;
        }

        protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            try
            {
                if (invokeValue.Action.Verb == "click")
                {
                    return await ProcessClick();
                }
                else if (invokeValue.Action.Verb == "back")
                {
                    return await ProcessBack();
                }
                else
                {
                    throw new InvokeResponseException(HttpStatusCode.NotImplemented);
                }
            }
            catch (AdaptiveCardActionException e)
            {
                throw new InvokeResponseException(HttpStatusCode.NotImplemented, e.Response);
            }
        }

        private Task<AdaptiveCardInvokeResponse> ProcessClick()
        {
            return Task.FromResult(new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(Path.Combine(".", "Cards", "CardTwo.json")).AsJObject()
            });
        }

        private Task<AdaptiveCardInvokeResponse> ProcessBack()
        {
            return Task.FromResult(new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(Path.Combine(".", "Cards", "CardOne.json")).AsJObject()
            });
        }
    }
}
