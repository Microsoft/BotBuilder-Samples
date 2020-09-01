using AdaptiveCards;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Botframework.AdaptiveCards.Converter.Facebook;
using Microsoft.Botframework.AdaptiveCards.Converter.LINE;
using Microsoft.Botframework.AdaptiveCards.Converter.Slack;
using Microsoft.Botframework.AdaptiveCards.Converter.Telegram;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AdaptiveCardsBot.Middleware
{
    public class AdaptiveCardConverterMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.OnSendActivities(SendActivitiesHandler);
            await next(cancellationToken);
        }
        public async Task<ResourceResponse[]> SendActivitiesHandler(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            foreach (var activity in activities)
            {
                if (activity.ChannelId == "line")
                {
                    var newList = new List<Attachment>();

                    var adaptiveCards = new List<AdaptiveCard>();
                    foreach (var attachment in activity.Attachments ?? new List<Attachment>())
                    {
                        if (attachment.ContentType == AdaptiveCard.ContentType)
                        {
                            var adaptiveCard = attachment.ContentAs<AdaptiveCard>();
                            adaptiveCards.Add(adaptiveCard);
                        }
                    }

                    var lineConverter = new LineCardConverter();
                    var flexMessages = await lineConverter.ToChannelData(adaptiveCards, activity.AttachmentLayout).ConfigureAwait(false);
                    var messageString = JsonConvert.SerializeObject(flexMessages, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    activity.ChannelData = JObject.Parse(messageString);
                    Console.WriteLine(messageString);

                    activity.Attachments = newList;
                }
                if (activity.ChannelId == "facebook")
                {
                    var newList = new List<Attachment>();
                    var adaptiveCards = new List<AdaptiveCard>();
                    foreach (var attachment in activity.Attachments ?? new List<Attachment>())
                    {
                        if (attachment.ContentType == AdaptiveCard.ContentType)
                        {
                            var adaptiveCard = attachment.ContentAs<AdaptiveCard>();
                            adaptiveCards.Add(adaptiveCard);
                        }
                        else
                        {
                            newList.Add(attachment);
                        }
                    }

                    var facebookConverter = new FacebookCardConverter();
                    var facebookChannelDataList = await facebookConverter.ToFacebookChannelData(adaptiveCards);
                    if (facebookChannelDataList.Count != 0)
                    {
                        if (facebookChannelDataList.Count == 1)
                        {
                            activity.ChannelData = facebookChannelDataList[0];

                        }
                        else
                        {
                            int i;
                            for (i = 0; i < facebookChannelDataList.Count - 1; i++)
                            {
                                IActivity newActivity = Activity.CreateMessageActivity();
                                newActivity.ChannelData = facebookChannelDataList[i];
                                await turnContext.SendActivityAsync(newActivity);
                            }
                            activity.ChannelData = facebookChannelDataList[i];
                        }
                    }
                    activity.Attachments = newList;
                }
                else if (activity.ChannelId == "slack")
                {
                    if (activity.Attachments.Any())
                    {
                        var newList = new List<Attachment>();
                        var adaptiveCards = new List<AdaptiveCard>();
                        foreach (var attachment in activity.Attachments ?? new List<Attachment>())
                        {
                            if (attachment.ContentType == AdaptiveCard.ContentType)
                            {
                                var adaptiveCard = attachment.ContentAs<AdaptiveCard>();
                                adaptiveCards.Add(adaptiveCard);
                            }
                            else
                            {
                                newList.Add(attachment);
                            }
                        }

                        var slackConverter = new SlackCardConverter();
                        var slackMessage = await slackConverter.ToChannelData(adaptiveCards).ConfigureAwait(false);
                        var messageString = JsonConvert.SerializeObject(slackMessage, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        activity.ChannelData = JObject.Parse(messageString);
                        activity.Attachments = newList;
                    }
                }
                else if (activity.ChannelId == "telegram")
                {
                    if (activity.Attachments.Any())
                    {
                        var adaptiveCards = new List<AdaptiveCard>();
                        foreach (var attachment in activity.Attachments ?? new List<Attachment>())
                        {
                            if (attachment.ContentType == AdaptiveCard.ContentType)
                            {
                                var adaptiveCard = attachment.ContentAs<AdaptiveCard>();
                                adaptiveCards.Add(adaptiveCard);
                            }
                        }

                        var telegramConverter = new TelegramCardConverter();
                        var telegramMethods = await telegramConverter.ToChannelData(adaptiveCards).ConfigureAwait(false);
                        activity.ChannelData = JArray.FromObject(telegramMethods);
                    }
                }
            }
            return await next();
        }
    }
}
