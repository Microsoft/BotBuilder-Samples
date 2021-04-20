// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class RootDialog : AdaptiveDialog
    {
        private Templates _templates;

        public RootDialog() : base(nameof(RootDialog))
        {
            string[] paths = { ".", "Dialogs", $"RootDialog.lg" };
            var fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);

            Recognizer = new RegexRecognizer()
            {
                Intents = new List<IntentPattern>()
                {
                    new IntentPattern()
                    {
                        Intent = "ChangeLang",
                        Pattern = "(language|speak|change)"
                    },
                    new IntentPattern()
                    {
                        Intent = "Help",
                        Pattern = "(help|options)"
                    },
                    new IntentPattern()
                    {
                        Intent = "Hero",
                        Pattern ="hero"
                    }
                }
            };

            Triggers = new List<OnCondition>
            {
                new OnConversationUpdateActivity()
                {
                    Actions = WelcomeUserSteps()
                },
                new OnIntent()
                {
                    Intent = "ChangeLang",
                    Actions= new List<Dialog>()
                    {
                        new TextInput()
                        {
                            AlwaysPrompt = true,
                            Property = "dialog.LanguagePreference",
                            Prompt = new ActivityTemplate("${LanguageChoicePrompt()}"),
                            InvalidPrompt = new ActivityTemplate("${InvalidChoice()}"),
                            Validations = new List<BoolExpression>()
                            {
                                new BoolExpression("this.value == 'en' || this.value == 'it' || this.value == 'fr' || this.value == 'es'")
                            }
                        },
                        new SetProperty()
                        {
                            Property= "user.LanguagePreference",
                            Value = "=dialog.LanguagePreference"
                        },
                       new SendActivity("${ShowSelection()}"),
                    }
                },
                new OnIntent()
                {
                    Intent = "Hero",
                    Actions = new List<Dialog>() { new SendActivity("${HeroCard()}") }
                },
                new OnIntent()
                {
                    Intent = "Help",
                    Actions = new List<Dialog>()
                    {
                        new SendActivity("${HelpInfo()}")
                    }
                },
                new OnUnknownIntent()
                {
                    Actions = new List<Dialog>()
                    {
                        new SendActivity("You said: '${turn.Activity.Text}'"),
                    }
                }
            };

            Generator = new TemplateEngineLanguageGenerator(_templates);
        }

        private static List<Dialog> WelcomeUserSteps() =>
            new List<Dialog>()
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "$foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("${AdaptiveCard()}"),
                                new SendActivity("${HelpInfo()}")
                            }
                        }
                    }
                }
            };
    }
}
