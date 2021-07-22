// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.BotBuilderSamples.Models;
using System;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsAdaptiveCardTabsBot : TeamsActivityHandler
    {
        private readonly string _connectionName;

        public TeamsAdaptiveCardTabsBot(IConfiguration config)
        {
            _connectionName = config["ConnectionName"];
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity?.Text?.Trim()?.ToLower() == "logout")
            {
                if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
                {
                    throw new InvalidOperationException("logout: not supported by the current adapter");
                }

                await adapter.SignOutUserAsync(turnContext, _connectionName, cancellationToken: cancellationToken).ConfigureAwait(false);
                var reply = MessageFactory.Text("You have been signed out.");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                var reply = MessageFactory.Text("Hello, I am a Teams Adaptive Card Tabs bot.  Please use the Tabs to interact. (Send 'logout' to sign out)");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }

        protected override async Task<TabResponse> OnTeamsTabFetchAsync(ITurnContext<IInvokeActivity> turnContext, TabRequest tabRequest, CancellationToken cancellationToken)
        {
            if (tabRequest.TabEntityContext.TabEntityId == "home")
            {
                var magicCode = string.Empty;
                if (!string.IsNullOrEmpty(tabRequest.State))
                {
                    if (int.TryParse(tabRequest.State, out int parsed))
                    {
                        magicCode = parsed.ToString();
                    }
                }

                return await GetPrimaryTabResponse(turnContext, magicCode, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return GetTabResponse(new[] { CardResources.Welcome, CardResources.InterviewCandidates, CardResources.VideoId });
            }
        }

        protected async override Task<TabResponse> OnTeamsTabSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TabSubmit tabSubmit, CancellationToken cancellationToken)
        {
            if (tabSubmit.TabEntityContext.TabEntityId == "home")
            {
                if (turnContext.Activity.Value is JObject logoutData && logoutData.ContainsKey("data"))
                {
                    if (logoutData["data"]["shouldLogout"] != null)
                    {
                        var credentials = turnContext.TurnState.Get<IConnectorClient>().Credentials as AppCredentials;
                        if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter) || credentials == null)
                        {
                            throw new InvalidOperationException("The current adapter does not support OAuth.");
                        }

                        await adapter.SignOutUserAsync(turnContext, credentials, _connectionName, cancellationToken: cancellationToken).ConfigureAwait(false);
                        return GetTabResponse(new[] { CardResources.Success });
                    }
                }

                var response = await GetPrimaryTabResponse(turnContext, null, cancellationToken).ConfigureAwait(false);
                // If the user is not signed in, the .Tab type will be auth
                var successCard = new TabResponseCard()
                {
                    Card = CreateAdaptiveCardAttachment(CardResources.Success)
                };
                response.Tab.Value.Cards.Insert(0, successCard);
                
                return response;
            }
            else
            {
                return GetTabResponse(new[] { CardResources.Success, CardResources.Welcome, CardResources.InterviewCandidates, CardResources.VideoId });
            }

        }

        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var taskInfo = new TaskModuleTaskInfo();
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var videoId = asJobject.GetValue("youTubeVideoId")?.ToString();
            if (!string.IsNullOrEmpty(videoId))
            {
                taskInfo.Url = taskInfo.FallbackUrl = "https://www.youtube.com/embed/" + videoId;
                taskInfo.Height = UIConstants.YouTube.Height;
                taskInfo.Width = UIConstants.YouTube.Width;
                taskInfo.Title = UIConstants.YouTube.Title.ToString();
            }
            else
            {
                // No video id is present, so return the InputText card.
                var attachment = new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = CreateAdaptiveCardAttachment(CardResources.InputText),
                };
                taskInfo.Card = attachment;
                taskInfo.Height = UIConstants.InputTextCard.Height;
                taskInfo.Width = UIConstants.InputTextCard.Width;
                taskInfo.Title = UIConstants.InputTextCard.Title.ToString();
            }
            
            return Task.FromResult(taskInfo.ToTaskModuleResponse());
        }

        protected override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            return Task.FromResult(TaskModuleResponseFactory.CreateResponse("Thanks!"));
        }

        private async Task<TabResponse> GetPrimaryTabResponse(ITurnContext turnContext, string magicCode, CancellationToken cancellationToken)
        {
            var credentials = turnContext.TurnState.Get<IConnectorClient>().Credentials as AppCredentials;
            if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter) || credentials == null)
            {
                throw new InvalidOperationException("The current adapter does not support OAuth.");
            }

            var token = await adapter.GetUserTokenAsync(turnContext, credentials, _connectionName, magicCode, cancellationToken).ConfigureAwait(false);
            // If a token is returned, the user is logged in
            if (!string.IsNullOrEmpty(token?.Token))
            {
                var cards = new[] { CardResources.QuickActions, CardResources.AdminConfig }
                                .Select(card => new TabResponseCard
                                {
                                    Card = CreateAdaptiveCardAttachment(card)
                                } as object).ToList();

                // add the manager card
                var user = await new SimpleGraphClient(token.Token).GetMeAsync().ConfigureAwait(false);
                var managerCard = new TabResponseCard
                {
                    Card = CreateAdaptiveCardAttachment(CardResources.ManagerDashboard, "{profileName}", user?.DisplayName ?? "[unknown]")
                };
                cards.Insert(1, managerCard);

                return new TabResponse
                {
                    Tab = new TabResponsePayload
                    {
                        Type = "continue",
                        Value = new TabResponseCards
                        {
                            Cards = cards
                        }
                    }
                };
            }

            // The user is not logged in, so send an 'auth' response.
            var signInResource = await adapter.GetSignInResourceAsync(turnContext, credentials, _connectionName, turnContext.Activity.From.Id, null, cancellationToken).ConfigureAwait(false);
            return new TabResponse
            {
                Tab = new TabResponsePayload
                {
                    Type = "auth",
                    SuggestedActions = new TabSuggestedActions()
                    {
                        Actions = new[]
                        {
                            new CardAction
                            {
                                Title = "Login",
                                Type = ActionTypes.OpenUrl,
                                Value = signInResource.SignInLink
                            }
                        }
                    }
                }
            };
        }

        private TabResponse GetTabResponse(string[] cardTypes)
        {
            var cards = cardTypes.Select(cardType => new TabResponseCard() { Card = CreateAdaptiveCardAttachment(cardType) }).ToArray();

            return new TabResponse
            {
                Tab = new TabResponsePayload
                {
                    Type = "continue",
                    Value = new TabResponseCards
                    {
                        Cards = cards
                    }
                }
            };
        }

        private object CreateAdaptiveCardAttachment(string card, string replaceText = null, string replacement = null)
        {
            var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.ToLower().EndsWith(card.ToLower()));

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCardJson = reader.ReadToEnd();
                    if(replaceText!=null && replacement != null)
                    {
                        adaptiveCardJson = adaptiveCardJson.Replace(replaceText, replacement);
                    }
                    return JsonConvert.DeserializeObject(adaptiveCardJson);
                }
            }
        }

    }
}
