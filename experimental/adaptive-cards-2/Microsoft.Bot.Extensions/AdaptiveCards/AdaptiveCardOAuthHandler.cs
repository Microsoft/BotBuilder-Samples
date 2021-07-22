// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Extensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.AdaptiveCards
{
    public class AdaptiveCardOAuthHandler
    {
        private readonly string _connectionName;
        private readonly string _text;
        private readonly string _title;
        private readonly AppCredentials _appCredentials;

        public AdaptiveCardOAuthHandler(string connectionName, string title, string text, AppCredentials appCredentials = null)
        {
            _connectionName = connectionName;
            _title = title;
            _text = text;
            _appCredentials = appCredentials;
        }

        public static bool IsOAuthInvoke(ITurnContext turnContext)
        {
            return IsVerifyStateInvoke(turnContext) || IsTokenExchangeInvoke(turnContext);
        }

        public async Task<AdaptiveCardOAuthResult> GetUserTokenAsync(ITurnContext turnContext, BotState botState, CancellationToken cancellationToken = default(CancellationToken))
        {
            var botIdentity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey);
            if (botIdentity == null || !botIdentity.Claims.Any(x => x.Type == AuthenticationConstants.AudienceClaim))
            {
                var identity = new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, _appCredentials.MicrosoftAppId)
                });
                turnContext.TurnState.Set<IIdentity>(BotAdapter.BotIdentityKey, identity);
            }

            if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
            {
                throw new InvalidOperationException("AdaptiveCardsOAuthHandler.GetUserTokenAsync(): not supported by the current adapter");
            }

            var state = await GetStateAsync(turnContext, botState, cancellationToken).ConfigureAwait(false);

            // Check the incoming activity to see if it's a signin/verifyCode or signin/tokenExchange invoke
            if (IsVerifyStateInvoke(turnContext))
            {
                if (!state.SentLoginRequest)
                {
                    return ErrorResult(HttpStatusCode.BadRequest, "Received a verify state without sending a loginRequest");
                }

                var stateObject = turnContext.Activity.Value as JObject;
                var stateValue = stateObject.GetValue("state", StringComparison.Ordinal)?.ToString();

                try
                {
                    var token = await adapter.GetUserTokenAsync(turnContext, _appCredentials, _connectionName, stateValue, cancellationToken).ConfigureAwait(false);

                    if (token != null)
                    {
                        return await TokenResult(turnContext, botState, state.OriginalActivity, token, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        return ErrorResult(HttpStatusCode.Unauthorized, "Incorrect verification code.");
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring exception for now and send internal server error, see comment above)
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    return ErrorResult(HttpStatusCode.InternalServerError, "Unable to call the Token Service to retrieve user token.");
                }
            }
            else if (IsTokenExchangeInvoke(turnContext))
            {
                if (!state.SentLoginRequest)
                {
                    return ErrorResult(HttpStatusCode.BadRequest, "Received a token exchange without sending a loginRequest");
                }

                var tokenExchangeRequest = ((JObject)turnContext.Activity.Value)?.ToObject<TokenExchangeInvokeRequest>();

                if (tokenExchangeRequest == null)
                {
                    return ErrorResult(HttpStatusCode.BadRequest, "The bot received an InvokeActivity that is missing a TokenExchangeInvokeRequest value. This is required to be sent with the InvokeActivity.");
                }
                else if (tokenExchangeRequest.ConnectionName != _connectionName)
                {
                    return ErrorResult(HttpStatusCode.BadRequest, "The bot received an InvokeActivity with a TokenExchangeInvokeRequest containing a ConnectionName that does not match the ConnectionName expected by the bot's active OAuthPrompt. Ensure these names match when sending the InvokeActivityInvalid ConnectionName in the TokenExchangeInvokeRequest");
                }
                else
                {
                    TokenResponse tokenExchangeResponse = null;
                    try
                    {
                        tokenExchangeResponse = await adapter.ExchangeTokenAsync(
                            turnContext,
                            _appCredentials,
                            _connectionName,
                            turnContext.Activity.From.Id,
                            new TokenExchangeRequest
                            {
                                Token = tokenExchangeRequest.Token,
                            },
                            cancellationToken).ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring, see comment below)
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // Ignore Exceptions
                        // If token exchange failed for any reason, tokenExchangeResponse above stays null , and hence we send back a failure invoke response to the caller.
                        // This ensures that the caller shows 
                    }

                    if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
                    {
                        return ErrorResult(HttpStatusCode.PreconditionFailed, "The bot is unable to exchange token. Proceed with regular login.");
                    }
                    else
                    {
                        var token = new TokenResponse
                        {
                            ChannelId = tokenExchangeResponse.ChannelId,
                            ConnectionName = tokenExchangeResponse.ConnectionName,
                            Token = tokenExchangeResponse.Token,
                        };
                        return await TokenResult(turnContext, botState, state.OriginalActivity, token, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            
            // try to get the user token, otherwise return a login request

            var tokenResponse = await adapter.GetUserTokenAsync(turnContext, _appCredentials, _connectionName, null, cancellationToken).ConfigureAwait(false);

            if (tokenResponse != null && !string.IsNullOrWhiteSpace(tokenResponse.Token))
            {
                return await TokenResult(turnContext, botState, turnContext.Activity, tokenResponse, cancellationToken).ConfigureAwait(false);
            }

            // else return the loginRequest

            var oauthCard = await CreateOAuthCard(turnContext, adapter, cancellationToken).ConfigureAwait(false);

            await SetStateAsync(turnContext, botState, true, cancellationToken).ConfigureAwait(false);

            return new AdaptiveCardOAuthResult()
            {
                InvokeResponse = new AdaptiveCardInvokeResponse()
                {
                    StatusCode = 401,
                    Type = Constants.LoginRequest,
                    Value = oauthCard
                }
            };
        }

        public async Task<AdaptiveCardOAuthResult> SignoutAsync(ITurnContext turnContext, BotState botState, CancellationToken cancellationToken = default(CancellationToken))
        {
            var botIdentity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey);
            if (botIdentity == null || !botIdentity.Claims.Any(x => x.Type == AuthenticationConstants.AudienceClaim))
            {
                var identity = new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, _appCredentials.MicrosoftAppId)
                });
                turnContext.TurnState.Set<IIdentity>(BotAdapter.BotIdentityKey, identity);
            }

            if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
            {
                throw new InvalidOperationException("AdaptiveCardsOAuthHandler.GetUserTokenAsync(): not supported by the current adapter");
            }

            try
            {
                await adapter.SignOutUserAsync(turnContext, _appCredentials, _connectionName, null, cancellationToken).ConfigureAwait(false);

            }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring exception for now and send internal server error, see comment above)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return ErrorResult(HttpStatusCode.InternalServerError, "Unable to call the Token Service to signout.");
            }
           
            return new AdaptiveCardOAuthResult()
            {
                InvokeResponse = new AdaptiveCardInvokeResponse()
                {
                    StatusCode = 200,
                    Type = Constants.Message,
                    Value = "Signout Successful"
                }
            };
        }

        public async Task ResetAsync(ITurnContext turnContext, BotState botState, CancellationToken cancellationToken = default(CancellationToken))
        {
            await ClearStateAsync(turnContext, botState, cancellationToken).ConfigureAwait(false);
        }

        private async Task<OAuthCard> CreateOAuthCard(ITurnContext turnContext, IExtendedUserTokenProvider adapter, CancellationToken cancellationToken)
        {
            var cardActionType = ActionTypes.Signin;
            var signInResource = await adapter.GetSignInResourceAsync(turnContext, _appCredentials, _connectionName, turnContext.Activity.From.Id, null, cancellationToken).ConfigureAwait(false);
            
            // use the SignInLink when 
            //   in speech channel or
            //   bot is a skill or
            //   an extra OAuthAppCredentials is being passed in
            if (turnContext.Activity.IsFromStreamingConnection() ||
                (turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && SkillValidation.IsSkillClaim(botIdentity.Claims)) ||
                _appCredentials != null)
            {
                if (turnContext.Activity.ChannelId == Channels.Emulator)
                {
                    cardActionType = ActionTypes.OpenUrl;
                }
            }

            return new OAuthCard
            {
                Text = _text,
                ConnectionName = _connectionName,
                Buttons = new[]
                {
                    new CardAction
                    {
                        Title = _title,
                        Text = _text,
                        Type = cardActionType,
                        Value = signInResource.SignInLink
                    },
                },
                TokenExchangeResource = signInResource.TokenExchangeResource,
            };
        }

        private static bool IsVerifyStateInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.VerifyStateOperationName;
        }

        private static bool IsTokenExchangeInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.TokenExchangeOperationName;
        }

        private static AdaptiveCardOAuthResult ErrorResult(HttpStatusCode statusCode, string errorMessage)
        {
            return new AdaptiveCardOAuthResult()
            {
                InvokeResponse = new AdaptiveCardInvokeResponse()
                {
                    StatusCode = (int)statusCode,
                    Type = Constants.Error,
                    Value = new Error()
                    {
                        Message = errorMessage
                    }
                }
            };
        }

        private async Task<AdaptiveCardOAuthResult> TokenResult(ITurnContext turnContext, BotState botState, Activity originalActivity, TokenResponse tokenResponse, CancellationToken cancellationToken)
        {
            await ClearStateAsync(turnContext, botState, cancellationToken);
            return new AdaptiveCardOAuthResult()
            {
                OriginalActivity = originalActivity,
                TokenResponse = tokenResponse
            };
        }

        private async Task SetStateAsync(ITurnContext turnContext, BotState botState, bool sentLoginRequest, CancellationToken cancellationToken)
        {
            var accessor = botState.CreateProperty<AdaptiveCardOAuthState>(nameof(AdaptiveCardOAuthState));

            await accessor.SetAsync(
                turnContext,
                new AdaptiveCardOAuthState()
                {
                    OriginalActivity = turnContext.Activity,
                    SentLoginRequest = sentLoginRequest
                },
                cancellationToken).ConfigureAwait(false);
        }

        private async Task ClearStateAsync(ITurnContext turnContext, BotState botState, CancellationToken cancellationToken)
        {
            var accessor = botState.CreateProperty<AdaptiveCardOAuthState>(nameof(AdaptiveCardOAuthState));

            await accessor.DeleteAsync(turnContext, cancellationToken);
        }

        private async Task<AdaptiveCardOAuthState> GetStateAsync(ITurnContext turnContext, BotState botState, CancellationToken cancellationToken)
        {
            var accessor = botState.CreateProperty<AdaptiveCardOAuthState>(nameof(AdaptiveCardOAuthState));

            return await accessor.GetAsync(
                turnContext,
                () => new AdaptiveCardOAuthState()
                {
                    OriginalActivity = null,
                    SentLoginRequest = false
                },
                cancellationToken).ConfigureAwait(false);
        }

        private class AdaptiveCardOAuthState
        {
            public Activity OriginalActivity { get; set; }

            public bool SentLoginRequest { get; set; }
        }
    }
}
