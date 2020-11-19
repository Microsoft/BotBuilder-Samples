// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        private Templates _templates;
        
        public MainDialog(ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;

            // combine path for cross platform support
            string[] paths = { ".", "Resources", "MainDialog.lg" };
            string mainDialogLGFile = Path.Combine(paths);

            // For simple LG resolution, we will call the Templates directly. 
            _templates = Templates.ParseFile(mainDialogLGFile);

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceCardStepAsync,
                ShowCardStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChoiceCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ChoiceCardStepAsync");

            // Create options for the prompt
            var options = new PromptOptions()
            {
                Prompt = ActivityFactory.FromObject(_templates.Evaluate("CardChoice")),
                Choices = new List<Choice>(),
            };

            // Add the choices for the prompt.
            options.Choices.Add(new Choice() { Value = "Adaptive card" });
            options.Choices.Add(new Choice() { Value = "Animation card" });
            options.Choices.Add(new Choice() { Value = "Audio card" });
            options.Choices.Add(new Choice() { Value = "Hero card" });
            options.Choices.Add(new Choice() { Value = "Receipt card" });
            options.Choices.Add(new Choice() { Value = "Signin card" });
            options.Choices.Add(new Choice() { Value = "Thumbnail card" });
            options.Choices.Add(new Choice() { Value = "Video card" });
            options.Choices.Add(new Choice() { Value = "All cards" });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ShowCardStepAsync");

            // Reply to the activity we received with an activity.
            var reply = stepContext.Context.Activity.CreateReply();

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments on the activity.
            reply.Attachments = new List<Attachment>();

            // Get the text from the activity to use to show the correct card
            var text = stepContext.Context.Activity.Text.ToLowerInvariant().Split(' ')[0];

            // Decide which type of card(s) we are going to show the user
            if (text.StartsWith("hero"))
            {
                // Display a HeroCard.
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("HeroCard")));
            }
            else if (text.StartsWith("thumb"))
            {
                // Display a ThumbnailCard.
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("ThumbnailCard")));
            }
            else if (text.StartsWith("sign"))
            {
                // Display a SignInCard.
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("SigninCard")));
            }
            else if (text.StartsWith("animation"))
            {
                // Display an AnimationCard.
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("AnimationCard")));
            }
            else if (text.StartsWith("video"))
            {
                // Display a VideoCard
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("VideoCard")));
            }
            else if (text.StartsWith("audio"))
            {
                // Display an AudioCard
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("AudioCard")));
            }
            else if (text.StartsWith("receipt"))
            {
                var data = new JObject
                {
                    ["receiptItems"] = JToken.FromObject(new List<ReceiptItem>
                    {
                        new ReceiptItem(
                            "Data Transfer",
                            price: "$ 38.45",
                            quantity: "368",
                            image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")),
                        new ReceiptItem(
                            "App Service",
                            price: "$ 45.00",
                            quantity: "720",
                            image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")),
                    })
                };
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("ReceiptCard", data)));
            }
            else if (text.StartsWith("adaptive"))
            {
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("AdaptiveCard")));
            }
            else
            {
                // Display a carousel of all the rich card types.
                await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("AllCards")));
            }

            // Give the user instructions about what to do next
            await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("CardStartOverResponse")), cancellationToken);

            return await stepContext.EndDialogAsync();
        }
    }
}
