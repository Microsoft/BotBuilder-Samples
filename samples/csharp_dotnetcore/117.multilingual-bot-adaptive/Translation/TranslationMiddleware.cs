// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Translation
{
    /// <summary>
    /// Middleware for translating text between the user and bot.
    /// Uses the Microsoft Translator Text API.
    /// </summary>
    public class TranslationMiddleware : IMiddleware
    {
        private readonly IStatePropertyAccessor<string> _languageStateProperty;
        private readonly MessageActivityTranslator _messageActivityTranslator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="translator">Translator implementation to be used for text translation.</param>
        /// <param name="userState">The UserState that contains the target language.</param>
        public TranslationMiddleware(MicrosoftTranslator translator, UserState userState, MessageActivityTranslator activityTranslator)
        {
            if(userState == null)
            {
                throw new ArgumentNullException(nameof(userState));
            }

            _languageStateProperty = userState.CreateProperty<string>("LanguagePreference");
            _messageActivityTranslator = activityTranslator ?? throw new ArgumentNullException(nameof(activityTranslator));
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var (translate, language) = await ShouldTranslateAsync(turnContext, cancellationToken);

            if (translate)
            {
                // This translates incoming messages from the user language to the default language that the bot understands.
                // Does not translate locale information and intent words
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    var text = turnContext.Activity.Text;
                    if (text != "en" && text != "fr" && text != "es" && text != "it" && text != "hero")
                    {
                        turnContext.Activity.Text = await _messageActivityTranslator.TranslateTextAsync(turnContext.Activity.Text, TranslationSettings.DefaultLanguage, cancellationToken);
                    }
                }
            }

            turnContext.OnSendActivities(async (newContext, activities, nextSend) =>
            {
                (translate, language) = await ShouldTranslateAsync(turnContext, cancellationToken);
                if (translate)
                {
                    var tasks = new List<Task>();
                    foreach (var activity in activities)
                    {
                        tasks.Add(_messageActivityTranslator.TranslateActivityAsync(activity, language,
                            cancellationToken));
                    }

                    if (tasks.Any())
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                }

                return await nextSend();
            });

            turnContext.OnUpdateActivity(async (newContext, activity, nextUpdate) =>
            {

                (translate, language) = await ShouldTranslateAsync(turnContext, cancellationToken);

                if (activity.Type == ActivityTypes.Message)
                {
                    if (translate)
                    {
                        await _messageActivityTranslator.TranslateActivityAsync(activity, language, cancellationToken);
                    }
                }

                return await nextUpdate();
            });

            await next(cancellationToken).ConfigureAwait(false);
        }


        private async Task<(bool, string)> ShouldTranslateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var userLanguage = await _languageStateProperty.GetAsync(turnContext, null, cancellationToken) ?? TranslationSettings.DefaultLanguage;
            return (userLanguage != TranslationSettings.DefaultLanguage, userLanguage);
        }
    }
}
