﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using WelcomeUser.State;

namespace WelcomeUser
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each interaction from the user, an instance of this class is created
    /// and the OnTurnAsync method called.
    /// Objects that are expensive to construct, or have a lifetime beyond a
    /// single turn, should be carefully managed and cached.
    /// </summary>
    public class WelcomeUserBot : IBot
    {
        // generic message sent to user
        private const string WelcomeMessage = @"This is a simple Welcome Bot sample.This bot will introduce you
                                                to welcoming and greeting users. You can say 'intro' to see the
                                                introduction card. If you are running this bot in the Bot Framework
                                                Emulator, press the 'Start Over' button to simulate user joining
                                                a bot or a channel";

        private const string InfoMessage = @"You are seeing this message because the bot recieved atleast one
                                            'ConversationUpdate' event, indicating you (and possibly others)
                                            joined the conversation. If you are using the emulator, pressing
                                            the 'Start Over' button to trigger this event again. The specifics
                                            of the 'ConversationUpdate' event depends on the channel. You can
                                            read more information at: 
                                             https://aka.ms/about-botframewor-welcome-user";

        private const string PatternMessage = @"It is a good pattern to use this event to send general greeting 
                                              to user, explaning what your bot can do. In this example, the bot 
                                              handles 'hello', 'hi', 'help' and 'intro. Try it now, type 'hi'";

        // The bot state accessor object. Use this to access specific state properties.
        private readonly WelcomeUserStateAccessors _welcomeUserStateAccessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeUserBot"/> class.
        /// </summary>
        /// <param name="statePropertyAccessor"> Bot state accessor object.</param>
        public WelcomeUserBot(WelcomeUserStateAccessors statePropertyAccessor)
        {
            _welcomeUserStateAccessors = statePropertyAccessor ?? throw new System.ArgumentNullException("state accessor can't be null");
        }

        /// <summary>
        /// Every conversation turn for our WelcomeUser Bot will call this method, including
        /// any type of activities such as ConversationUpdate or ContactRelationUpdate which
        /// are sent when a user joins a conversation.
        /// This bot doesn't use any dialogs; it's "single turn" processing, meaning a single
        /// request and response.
        /// This bot uses UserState to keep track of first message a user sends.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {
            // use state accessor to extract the didBotWelcomeUser flag
            var didBotWelcomeUser = await _welcomeUserStateAccessors.DidBotWelcomedUser.GetAsync(turnContext, () => false);

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Your bot should proactively send a welcome message to a personal chat the first time
                // (and only the first time) a user initiates a personal chat with your bot.
                if (didBotWelcomeUser == false)
                {
                    // Update user state flag to reflect bot handled first user interaction.
                    await _welcomeUserStateAccessors.DidBotWelcomedUser.SetAsync(turnContext, true);
                    await _welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);

                    // the channel should sends the user name in the 'From' object
                    var userName = turnContext.Activity.From.Name;

                    await turnContext.SendActivityAsync($"You are seeing this message because this was your first message ever to this bot.", cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync($"It is a good practice to welcome the user and provdie personal greeting. For example, welcome {userName}.", cancellationToken: cancellationToken);
                }
                else
                {
                    // This example hardcodes specific uterances. You should use LUIS or QnA for more advance language understanding.
                    var text = turnContext.Activity.Text.ToLowerInvariant();
                    switch (text)
                    {
                        case "hello":
                        case "hi":
                            await turnContext.SendActivityAsync($"You said {text}.", cancellationToken: cancellationToken);
                            break;
                        case "intro":
                        case "help":
                            await SendIntroCardAsync(turnContext, cancellationToken);
                            break;
                        default:
                            await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                            break;
                    }
                }
            }

            // Greet when users are added to the conversation.
            // Note that all channels do not send the conversation update activity.
            // If you find that this bot works in the emulator, but does not in
            // another channel the reason is most likely that the channel does not
            // send this activity.
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded.Any())
                {
                    // Iterate over all new members added to the conversation
                    foreach (var member in turnContext.Activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message
                        // the 'bot' is the recipient for events from the channel,
                        // turnContext.Activity.MembersAdded == turnContext.Activity.Recipient.Id indicates the
                        // bot was added to the conversation.
                        if (member.Id != turnContext.Activity.Recipient.Id)
                        {
                            await turnContext.SendActivityAsync($"Hi there - {member.Name}. {WelcomeMessage}", cancellationToken: cancellationToken);
                            await turnContext.SendActivityAsync(InfoMessage, cancellationToken: cancellationToken);
                            await turnContext.SendActivityAsync(PatternMessage, cancellationToken: cancellationToken);
                        }
                    }
                }
            }
            else
            {
                // Default behaivor for all other type of activities.
                var activity = turnContext.Activity;
                await turnContext.SendActivityAsync($"Received activity: {activity.Type}");
            }

            // save any state changes made to your state objects.
            await _welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);
        }

        /// <summary>
        /// Sends an adaptive card greeting.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var response = turnContext.Activity.CreateReply();

            // Create a HeroCard to send to the usergit 
            var card = new HeroCard
            {
                Title = "Welcome to Bot Framework!",
                Text = @"Welcome to Welcome Users bot sample! This Introduction card 
                         is a great way to introduce your Bot to the user and suggest 
                         some things to get them started. We use this opportunity to 
                         recommend a few next steps for learning more creating and deploying bots.",
                Images = new List<CardImage>() {new CardImage("https://aka.ms/bf-welcome-card-image")},
                Buttons = new List<CardAction>()
                {
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "Get an overview",
                        null,
                        "Get an overview",
                        "Get an overview",
                        "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "Ask a question",
                        null,
                        "Ask a question",
                        "Ask a question",
                        "https://stackoverflow.com/questions/tagged/botframework"),
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "Learn how to deploy",
                        null,
                        "Learn how to deploy",
                        "Learn how to deploy",
                        "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
                },
            };

            response.Attachments = new List<Attachment>() { card.ToAttachment() };
            await turnContext.SendActivityAsync(response, cancellationToken);
        }
    }
}
