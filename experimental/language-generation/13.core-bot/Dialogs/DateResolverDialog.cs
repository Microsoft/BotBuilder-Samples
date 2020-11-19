// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder;
using System.IO;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class DateResolverDialog : CancelAndHelpDialog
    {
        private Templates _templates;

        public DateResolverDialog(string id = null)
            : base(id ?? nameof(DateResolverDialog))
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "BookingDialog.lg" };
            string fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);

            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), DateTimePromptValidator));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            var timex = bookingDetails.TravelDate;

            var promptMsg = _templates.Evaluate("PromptForTravelDate");
            var repromptMsg = _templates.Evaluate("InvalidDateReprompt");

            if (timex == null)
            {
                // We were not given any date at all so prompt the user.
                return await stepContext.PromptAsync(nameof(DateTimePrompt),
                    new PromptOptions
                    {
                        Prompt = ActivityFactory.FromObject(promptMsg),
                        RetryPrompt = ActivityFactory.FromObject(repromptMsg)
                    }, cancellationToken);
            }
            else
            {
                // We have a Date we just need to check it is unambiguous.
                var timexProperty = new TimexProperty(timex);
                if (!timexProperty.Types.Contains(Constants.TimexTypes.Definite))
                {
                    // This is essentially a "reprompt" of the data we were given up front.
                    return await stepContext.PromptAsync(nameof(DateTimePrompt),
                        new PromptOptions
                        {
                            Prompt = ActivityFactory.FromObject(repromptMsg)
                        }, cancellationToken);
                }
                else
                {
                    return await stepContext.NextAsync(new DateTimeResolution { Timex = timex }, cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var timex = ((List<DateTimeResolution>)stepContext.Result)[0].Timex;
            return await stepContext.EndDialogAsync(timex, cancellationToken);
        }

        private static Task<bool> DateTimePromptValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                var timex = promptContext.Recognized.Value[0].Timex.Split('T')[0];

                // If this is a definite Date including year, month and day we are good otherwise reprompt.
                // A better solution might be to let the user know what part is actually missing.
                var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);

                return Task.FromResult(isDefinite);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
