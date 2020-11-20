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
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.BotBuilderSamples
{
    public class UserProfileDialog : AdaptiveDialog
    {
        private readonly Templates _templates;

        public UserProfileDialog() : base(nameof(UserProfileDialog))
        {
            string[] paths = { ".", "Dialogs", $"UserProfileDialog.lg" };
            var fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);

            // These steps are executed when this Adaptive Dialog begins
            Triggers = new List<OnCondition>
            {
                // Add a rule to welcome user
                new OnConversationUpdateActivity
                {
                    Actions = WelcomeUserSteps()
                },
                // Respond to user on message activity
                new OnUnknownIntent
                {
                    Actions = GatheUserInformation()
                },
            };
            Generator = new TemplateEngineLanguageGenerator(_templates);
        }

        private static List<Dialog> WelcomeUserSteps()
        {
            return new List<Dialog>
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message. 
                        new IfCondition
                        {
                            Condition = "$foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>
                            {
                                new SendActivity("${WelcomeMessage()}")
                            }
                        }
                    }
                }
            };

        }

        private Dialog GetTransport()
        {
            return new ChoiceInput
            {
                // Ask for user's transportation and set it in user.userProfile scope.
                Prompt = new ActivityTemplate("${ModeOfTransportPrompt()}"),
                Style = ListStyle.Auto,
                // Inputs will skip the prompt if the property (turn.cardChoice in this case) already has value.
                // Since we are using RepeatDialog, we will set AlwaysPrompt property so we do not skip this prompt
                // and end up in an infinite loop.
                AlwaysPrompt = true,
                Choices = new ObjectExpression<ChoiceSet>(_templates.Evaluate("TransportChoices") as string),
                Property = "user.userProfile.Transport"
            };
        }

        private Dialog GetAge()
        {
            return new IfCondition
            {
                // All conditions are expressed using adaptive expressions.
                // See https://aka.ms/adaptive-expressions to learn more
                Condition = "turn.ageConfirmation == true",
                Actions = new List<Dialog>
                {
                    new NumberInput
                    {
                        Prompt = new ActivityTemplate("${AskForAge()}"),
                        Property = "user.userProfile.Age",
                        // Add validations
                        Validations = new List<BoolExpression>
                        {
                            // Age must be greater than or equal 1
                            "int(this.value) >= 1",
                            // Age must be less than 150
                            "int(this.value) < 150"
                        },
                        InvalidPrompt = new ActivityTemplate("${AskForAge.invalid()}"),
                        UnrecognizedPrompt = new ActivityTemplate("${AskForAge.unRecognized()}")
                    },
                    new SendActivity("${UserAgeReadBack()}")
                },
                ElseActions = new List<Dialog>
                {
                    new SendActivity("${NoAge()}")
                }
            };
        }

        private Dialog GetProfilePicture()
        {
            return new IfCondition
            {
                // Teams file upload is different from other channels, so is not
                // included in this sample.
                Condition = "turn.activity.channelId == 'msteams'",
                Actions = new List<Dialog>
                {
                    new SendActivity("${TeamsFileUpload()}"),
                },
                ElseActions = new List<Dialog>
                {
                    new AttachmentOrTextInput
                    {
                        Prompt = new ActivityTemplate("${PictureUploadPrompt()}"),
                        InvalidPrompt = new ActivityTemplate("${InvalidPicture()}"),
                        Validations = new List<BoolExpression>
                        {
                            // We provide two options for the user:
                            //   1) no attachment uploaded (skip)
                            //   2) an attachment upload of type png or jpeg
                            "(turn.activity.attachments == null || turn.activity.attachments.count == 0) || (turn.activity.attachments[0].contentType == 'image/jpeg' || turn.activity.attachments[0].contentType == 'image/png')",
                        },
                        Property = "user.picture"
                    },
                    new IfCondition
                    {
                        Condition = "user.picture != null",
                        Actions = new List<Dialog>
                        {
                            new SendActivity("${ThanksForPicture()}"),
                        },
                        ElseActions = new List<Dialog>
                        {
                            new SendActivity("${NoPicture()}"),
                        }
                    }
                }
            };
        }

        private List<Dialog> GatheUserInformation()
        {
            return new List<Dialog>
            {
                GetTransport(),
                new TextInput()
                {
                    Prompt = new ActivityTemplate("${AskForName()}"),
                    Property = "user.userProfile.Name"
                },
                // SendActivity supports full language generation resolution.
                // See here to learn more about language generation
                // https://aka.ms/language-generation
                new SendActivity("${AckName()}"),
                new ConfirmInput
                {
                    Prompt = new ActivityTemplate("${AgeConfirmPrompt()}"),
                    Property = "turn.ageConfirmation"
                },
                GetAge(),
                GetProfilePicture(),
                new ConfirmInput
                {
                    Prompt = new ActivityTemplate("${FinalConfirmPrompt()}"),
                    Property = "turn.finalConfirmation"
                },
                // Use LG template to come back with the final read out.
                // This LG template is a great example of what logic can be wrapped up in LG sub-system.
                new SendActivity("${FinalUserProfileReadOut()}"), // examines turn.finalConfirmation
                new IfCondition
                {
                    Condition = "user.picture != null && turn.finalConfirmation",
                    Actions = new List<Dialog>
                    {
                        new SendActivity("${PictureConfirmation()}"),
                    }
                },
                new DeleteProperty
                {
                    Property = "user.userProfile"
                },
                new EndDialog(),
            };
        }
    }
}
