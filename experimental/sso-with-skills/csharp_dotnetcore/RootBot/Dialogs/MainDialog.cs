// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.RootBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public static readonly string ActiveSkillPropertyName = $"{typeof(MainDialog).FullName}.ActiveSkillProperty";
        private readonly IStatePropertyAccessor<BotFrameworkSkill> _activeSkillProperty;
        private readonly string _connectionName;
        private readonly BotFrameworkSkill _ssoSkill;

        public MainDialog(ConversationState conversationState, SkillsConfiguration skillsConfig, SkillHttpClient skillClient, IConfiguration configuration, SkillConversationIdFactoryBase conversationIdFactory)
            : base(nameof(MainDialog))
        {
            var botId = configuration.GetSection("MicrosoftAppId")?.Value;
            if (string.IsNullOrWhiteSpace(botId))
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is not set in configuration");
            }

            _connectionName = configuration.GetSection("ConnectionName")?.Value;
            if (string.IsNullOrWhiteSpace(_connectionName))
            {
                throw new ArgumentException("\"ConnectionName\" is not set in configuration");
            }

            // We use a single skill in this example.
            var targetSkillId = "SkillBot";
            if (!skillsConfig.Skills.TryGetValue(targetSkillId, out _ssoSkill))
            {
                throw new ArgumentException($"Skill with ID \"{targetSkillId}\" not found in configuration");
            }

            AddDialog(new ChoicePrompt("ActionStepPrompt"));
            AddDialog(new SsoSignInDialog(_connectionName));
            AddDialog(new SkillDialog(CreateSkillDialogOptions(skillsConfig, botId, conversationIdFactory, conversationState, skillClient),  nameof(SkillDialog)));

            var waterfallSteps = new WaterfallStep[] { PromptActionStepAsync, HandleActionStepAsync, PromptFinalStepAsync, };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // Create state property to track the active skill.
            _activeSkillProperty = conversationState.CreateProperty<BotFrameworkSkill>(ActiveSkillPropertyName);

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Helper to create a SkillDialogOptions instance for the SSO skill.
        private SkillDialogOptions CreateSkillDialogOptions(SkillsConfiguration skillsConfig, string botId, SkillConversationIdFactoryBase conversationIdFactory, ConversationState conversationState, BotFrameworkClient skillClient)
        {
            return new SkillDialogOptions
            {
                BotId = botId,
                ConversationIdFactory = conversationIdFactory,
                ConversationState = conversationState,
                Skill = _ssoSkill,
                SkillClient = skillClient,
                SkillHostEndpoint = skillsConfig.SkillHostEndpoint
            };
        }

        private async Task<DialogTurnResult> PromptActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = "What SSO action do you want to perform?";
            var repromptMessageText = "That was not a valid choice, please select a valid choice.";
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput),
                RetryPrompt = MessageFactory.Text(repromptMessageText, repromptMessageText, InputHints.ExpectingInput),
                Choices = await GetPromptChoicesAsync(stepContext, cancellationToken)
            };

            // Prompt the user to select a skill.
            return await stepContext.PromptAsync("ActionStepPrompt", options, cancellationToken);
        }

        // Creates the prompt choices based on the current sign in status
        private async Task<List<Choice>> GetPromptChoicesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptChoices = new List<Choice>();
            var adapter = (IUserTokenProvider)stepContext.Context.Adapter;

            // Show different options if the user is signed in on the parent or not.
            var token = await adapter.GetUserTokenAsync(stepContext.Context, _connectionName, null, cancellationToken);
            if (token == null)
            {
                // User is not signed in.
                promptChoices.Add(new Choice("Login to the root bot"));
                
                // Token exchange will fail when the root is not logged on and the skill should 
                // show a regular OAuthPrompt
                promptChoices.Add(new Choice("Call Skill (without SSO)"));
            }
            else
            {
                // User is signed in to the parent.
                promptChoices.Add(new Choice("Logout from the root bot"));
                promptChoices.Add(new Choice("Show token"));
                promptChoices.Add(new Choice("Call Skill (with SSO)"));
            }

            return promptChoices;
        }

        private async Task<DialogTurnResult> HandleActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var action = ((FoundChoice)stepContext.Result).Value.ToLowerInvariant();

            switch (action)
            {
                case "login to the root bot":
                    return await stepContext.BeginDialogAsync(nameof(SsoSignInDialog), null, cancellationToken);

                case "logout from the root bot":
                    var adapter = (IUserTokenProvider)stepContext.Context.Adapter;
                    await adapter.SignOutUserAsync(stepContext.Context, _connectionName, cancellationToken: cancellationToken);
                    await stepContext.Context.SendActivityAsync("You have been signed out.", cancellationToken: cancellationToken);
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                case "show token":
                    var tokenProvider = (IUserTokenProvider)stepContext.Context.Adapter;
                    var token = await tokenProvider.GetUserTokenAsync(stepContext.Context, _connectionName, null, cancellationToken);
                    if (token == null)
                    {
                        await stepContext.Context.SendActivityAsync("User has no cached token.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Here is your current SSO token: {token.Token}", cancellationToken: cancellationToken);
                    }

                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                case "call skill (with sso)":
                case "call skill (without sso)":
                    var beginSkillActivity = new Activity
                    {
                        Type = ActivityTypes.Event,
                        Name = "Sso"
                    };

                    // Save active skill in state (this is use in case of errors in the AdapterWithErrorHandler).
                    await _activeSkillProperty.SetAsync(stepContext.Context, _ssoSkill, cancellationToken);

                    return await stepContext.BeginDialogAsync(nameof(SkillDialog), new BeginSkillDialogOptions { Activity = beginSkillActivity }, cancellationToken);

                default:
                    // This should never be hit since the previous prompt validates the choice
                    throw new InvalidOperationException($"Unrecognized action: {action}");
            }
        }

        private async Task<DialogTurnResult> PromptFinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Clear active skill in state.
            await _activeSkillProperty.DeleteAsync(stepContext.Context, cancellationToken);

            // Restart the dialog (we will exit when the user says end)
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }
    }
}
