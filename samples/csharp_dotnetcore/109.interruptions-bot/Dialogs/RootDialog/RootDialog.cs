// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples
{
    public class RootDialog : ComponentDialog
    {
        private readonly IConfiguration configuration;
        private Templates _templates;

        public RootDialog(IConfiguration configuration)
            : base(nameof(RootDialog))
        {
            this.configuration = configuration;
            _templates = Templates.ParseFile(Path.Combine(".", "Dialogs", "RootDialog", "RootDialog.lg"));

            // Create instance of adaptive dialog.
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Recognizer = CreateLuisRecognizer(this.configuration),
                Generator = new TemplateEngineLanguageGenerator(_templates),
                Triggers = new List<OnCondition>()
                {
                    new OnConversationUpdateActivity()
                    {
                        Actions = WelcomeUserActions()
                    },
                    new OnIntent()
                    {
                        Intent = "GetUserProfile",
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog()
                            {
                                Dialog = nameof(GetUserProfileDialog)
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Help",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${RootHelp()}")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Cancel",
                        Actions = new List<Dialog>()
                        {
                            new ConfirmInput()
                            {
                                Property = "turn.confirm",
                                AllowInterruptions = false,
                                Prompt = new ActivityTemplate("${RootCancelConfirm()}")
                            },
                            new IfCondition()
                            {
                                Condition = "turn.confirm == true",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("${CancelReadBack()}"),
                                    new CancelAllDialogs()
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("${Cancelcancelled()}")
                                }
                            }
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            AddDialog(new GetUserProfileDialog(this.configuration));

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static Recognizer CreateLuisRecognizer(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["luis:RootDialog_en_us_lu"]) || string.IsNullOrEmpty(configuration["luis:LuisAPIKey"]) || string.IsNullOrEmpty(configuration["luis:LuisAPIHostName"]))
            {
                throw new Exception("NOTE: LUIS is not configured for RootDialog. To enable all capabilities, add 'LuisAppId-RootDialog', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.");
            }

            return new LuisAdaptiveRecognizer()
            {
                ApplicationId = configuration["luis:RootDialog_en_us_lu"],
                EndpointKey = configuration["luis:LuisAPIKey"],
                Endpoint = configuration["luis:LuisAPIHostName"]
            };
        }

        private static List<Dialog> WelcomeUserActions()
        {
            return new List<Dialog>()
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>()
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message. 
                        new IfCondition()
                        {
                            Condition = "$foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("${WelcomeUser()}"),
                                new SetProperty()
                                {
                                    Property = "user.profile",
                                    Value = "={}"
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
