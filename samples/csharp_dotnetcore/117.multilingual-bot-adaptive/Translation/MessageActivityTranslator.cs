// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Translation
{
    public class MessageActivityTranslator
    {
        private readonly MicrosoftTranslator _translator;

        public MessageActivityTranslator(MicrosoftTranslator translator)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        /// <summary>
        /// Translates supported elements of an activity.
        /// </summary>
        /// <param name="activity">The message activity to translate.</param>
        /// <param name="targetLocale">The target locale to translate to.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        public async Task TranslateActivityAsync(IMessageActivity activity, string targetLocale,
            CancellationToken cancellationToken = default)
        {
            if (activity.Type != ActivityTypes.Message)
            {
                return;
            }

            if (activity.Text != null)
            {
                activity.Text = await TranslateTextAsync(activity.Text, targetLocale, cancellationToken);
            }

            if (activity.SuggestedActions!=null && activity.SuggestedActions.Actions.Any())
            {
                await TranslateCardActionsAsync(activity.SuggestedActions.Actions, targetLocale, cancellationToken);
            }

            if (activity.Attachments != null && activity.Attachments.Any())
            {
                IList<Attachment> attachments = new List<Attachment>();

                foreach (var attachment in activity.Attachments)
                {
                    switch (attachment.ContentType)
                    {
                        case HeroCard.ContentType:
                            var heroCard = ((JObject)attachment.Content).ToObject<HeroCard>();
                            var translatedCard = await TranslateHeroCardAsync(heroCard, targetLocale, cancellationToken);
                            AddInAttachment(attachments, translatedCard, HeroCard.ContentType);
                            break;
                        default:
                            //No changes for unsupported types. Add your own translation methods as needed.
                            attachments.Add(attachment);
                            break;
                    }
                }

                activity.Attachments = attachments;
            }
        }

        public async Task<string> TranslateTextAsync(string text, string targetLocale, CancellationToken cancellationToken = default(CancellationToken)) => await _translator.TranslateAsync(text, targetLocale, cancellationToken);

        private async Task<HeroCard> TranslateHeroCardAsync(HeroCard card, string targetLocale, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(card.Title))
            {
                card.Title = await TranslateTextAsync(card.Title, targetLocale, cancellationToken);
            }

            if (!string.IsNullOrEmpty(card.Subtitle))
            {
                card.Subtitle = await TranslateTextAsync(card.Subtitle, targetLocale, cancellationToken);
            }

            if (!string.IsNullOrEmpty(card.Text))
            {
                card.Text = await TranslateTextAsync(card.Text, targetLocale, cancellationToken);
            }

            if (card.Buttons != null && card.Buttons.Any())
            {
                await TranslateCardActionsAsync(card.Buttons, targetLocale, cancellationToken);
            }

            return card;

        }

        private async Task TranslateCardActionsAsync(IEnumerable<CardAction> cardActions, string targetLocale, CancellationToken cancellationToken)
        {
            foreach (var cardAction in cardActions)
            {
                if( !string.IsNullOrEmpty(cardAction.Text))
                {
                    cardAction.Text = await TranslateTextAsync(cardAction.Text, targetLocale, cancellationToken);
                }

                if (!string.IsNullOrEmpty(cardAction.Title))
                {
                    cardAction.Title = await TranslateTextAsync(cardAction.Title, targetLocale, cancellationToken);
                }
            }
        }

        private static void AddInAttachment(ICollection<Attachment> attachments, HeroCard translatedCard, string attachmentType)
        {
            var attachment = new Attachment {ContentType = attachmentType, Content = translatedCard};
            attachments.Add(attachment);
        }
    }
}
