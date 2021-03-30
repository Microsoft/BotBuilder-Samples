// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsMessagingExtensionsActionBot : TeamsActivityHandler
    {
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            switch (action.CommandId)
            {
                case "createCard":
                    return CreateCardCommand(turnContext, action);
                case "shareMessage":
                    return ShareMessageCommand(turnContext, action);
                case "webView":
                    return CreateThumbnailCard(turnContext, action);
                case "HTML":
                    return CreateHeroCard(turnContext, action);
                case "createAdaptiveCard":
                    return CreateAdaptiveCard(turnContext, action);
                case "razorView":
                    return CreateRazorViewCard(turnContext, action);
            }
            return await Task.FromResult(new MessagingExtensionActionResponse());
        }

        private MessagingExtensionActionResponse CreateRazorViewCard(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            // The user has chosen to create a card by choosing the 'Create Card' context menu command.
            CreateRazorCardData cardData = JsonConvert.DeserializeObject<CreateRazorCardData>(action.Data.ToString());
            var card = new HeroCard
            {
                Title = "Requested User: " + turnContext.Activity.From.Name,
                Subtitle = cardData.Title,
                Text = cardData.DisplayData,
            };

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = HeroCard.ContentType,
                Preview = card.ToAttachment(),
            });

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse CreateCardCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            // The user has chosen to create a card by choosing the 'Create Card' context menu command.
            var createCardData = ((JObject)action.Data).ToObject<CreateCardData>();

            var card = new HeroCard
            {
                Title = createCardData.Title,
                Subtitle = createCardData.Subtitle,
                Text = createCardData.Text,
            };

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = HeroCard.ContentType,
                Preview = card.ToAttachment(),
            });

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse ShareMessageCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            // The user has chosen to share a message by choosing the 'Share Message' context menu command.
            var heroCard = new HeroCard
            {
                Title = $"{action.MessagePayload.From?.User?.DisplayName} orignally sent this message:",
                Text = action.MessagePayload.Body.Content,
            };

            if (action.MessagePayload.Attachments != null && action.MessagePayload.Attachments.Count > 0)
            {
                // This sample does not add the MessagePayload Attachments.  This is left as an
                // exercise for the user.
                heroCard.Subtitle = $"({action.MessagePayload.Attachments.Count} Attachments not included)";
            }

            // This Messaging Extension example allows the user to check a box to include an image with the
            // shared message.  This demonstrates sending custom parameters along with the message payload.
            var includeImage = ((JObject)action.Data)["includeImage"]?.ToString();
            if (string.Equals(includeImage, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                heroCard.Images = new List<CardImage>
                {
                    new CardImage { Url = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU" },
                };
            }

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment>()
                    {
                        new MessagingExtensionAttachment
                        {
                            Content = heroCard,
                            ContentType = HeroCard.ContentType,
                            Preview = heroCard.ToAttachment(),
                        },
                    },
                },
            };
        }

        private MessagingExtensionActionResponse CreateThumbnailCard(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            // The user has chosen to create a card by choosing the 'Create Card' context menu command.
            CreatThumbnailCardData cardData = JsonConvert.DeserializeObject<CreatThumbnailCardData>(action.Data.ToString());
            var card = new ThumbnailCard
            {
                Title = "ID: " + cardData.EmpId,
                Subtitle = "Name: " + cardData.EmpName,
                Text = "E-Mail: " + cardData.EmpEmail,
                Images = new List<CardImage> { new CardImage("https://3er1viui9wo30pkxh1v2nh4w-wpengine.netdna-ssl.com/wp-content/uploads/prod/2014/10/MSFT_logo_rgb_C-Gray-768x283.png") },
            };

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = ThumbnailCard.ContentType,
                Preview = card.ToAttachment(),
            });

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse CreateHeroCard(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            // The user has chosen to create a card by choosing the 'Create Card' context menu command.
            CreatHeroCardData cardData = JsonConvert.DeserializeObject<CreatHeroCardData>(action.Data.ToString());
            var card = new HeroCard
            {
                Title = "User Name: " + cardData.UserName,
                Text = "Successfully logged into " + cardData.UserName + " profile",
            };

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = HeroCard.ContentType,
                Preview = card.ToAttachment(),
            });

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private MessagingExtensionActionResponse CreateAdaptiveCard(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var createCardData = ((JObject)action.Data).ToObject<CreateCardData>();
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            adaptiveCard.Body = new List<AdaptiveElement>()
            {
                new AdaptiveColumnSet()
                {
                    Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {
                            Items=new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "Name :",
                                    Wrap=true,
                                    Size=AdaptiveTextSize.Medium,
                                    Weight=AdaptiveTextWeight.Bolder
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        },
                         new AdaptiveColumn()
                        {
                            Items=new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= createCardData.Title,
                                    Wrap=true,
                                    Size=AdaptiveTextSize.Medium,
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        },
                    }
                },
                new AdaptiveColumnSet()
                {
                    Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {
                            Items=new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "Designation :",
                                    Wrap=true,
                                    Size=AdaptiveTextSize.Medium,
                                    Weight=AdaptiveTextWeight.Bolder
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        },
                        new AdaptiveColumn()
                        {
                            Items=new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= createCardData.Subtitle,
                                    Wrap=true,
                                    Size=AdaptiveTextSize.Medium,
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        },
                    }
                },
                new AdaptiveColumnSet()
                {
                    Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {
                            Items=new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "Description :",
                                    Wrap=true,
                                    Size=AdaptiveTextSize.Medium,
                                    Weight=AdaptiveTextWeight.Bolder
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        },
                         new AdaptiveColumn()
                        {
                            Items=new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= createCardData.Text,
                                    Wrap=true,
                                    Size=AdaptiveTextSize.Medium,
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        },
                    }
                },
            };

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = adaptiveCard,
                ContentType = AdaptiveCard.ContentType
               
            });

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        private class CreateRazorCardData
        {
            public string Title { get; set; }
            public string DisplayData { get; set; }
        }

        private class CreateCardData
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Text { get; set; }
        }

        private class CreatThumbnailCardData
        {
            public string EmpId { get; set; }
            public string EmpName { get; set; }
            public string EmpEmail { get; set; }
        }
        
        private class CreatHeroCardData
        {
            public string UserName { get; set; }
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            switch (action.CommandId)
            {
                case "webView":
                    return webView(turnContext, action);
                case "HTML":
                    return htmlMethod(turnContext, action);
                case "razorView":
                    return razorMethod(turnContext, action);
                default:
                    // we are handling two cases within try/catch block 
                    //if the bot is installed it will create adaptive card attachment and show card with input fields
                    string memberName;
                    try
                    {
                        // Check if your app is installed by fetching member information.
                        var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                        memberName = member.Name;
                    }
                    catch (ErrorResponseException ex)
                    {
                        if (ex.Body.Error.Code == "BotNotInConversationRoster")
                        {
                            return new MessagingExtensionActionResponse
                            {
                                Task = new TaskModuleContinueResponse
                                {
                                    Value = new TaskModuleTaskInfo
                                    {
                                        Card = GetAdaptiveCardAttachmentFromFile("justintimeinstallation.json"),
                                        Height = 200,
                                        Width = 400,
                                        Title = "Adaptive Card - App Installation",
                                    },
                                },
                            };
                        }
                        throw; // It's a different error.
                    }
                    return new MessagingExtensionActionResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo
                            {
                                Card = GetAdaptiveCardAttachmentFromFile("adaptiveCard.json"),
                                Height = 200,
                                Width = 400,
                                Title = $"Welcome {memberName}",
                            },
                        },
                    };
            }
        }

        private MessagingExtensionActionResponse razorMethod(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 300,
                        Width = 300,
                        Title = "Task Module Razor View",
                        Url = "YourDeployedBotUrl" + "/Home/RazorView",
                    },
                },
            };
            return response;
        }

        private MessagingExtensionActionResponse htmlMethod(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 300,
                        Width = 300,
                        Title = "Task Module HTML",
                        Url = "YourDeployedBotUrl" + "/htmlpage",
                    },
                },
            };
            return response;
        }

        private MessagingExtensionActionResponse webView (ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 500,
                        Width = 450,
                        Title = "Task module WebView",
                        Url = "YourDeployedBotUrl" + "/CustomForm",
                    },
                },
            };
            return response;
        }

        private static Attachment GetAdaptiveCardAttachmentFromFile(string fileName)
        {
            //Read the card json and create attachment.
            string[] paths = { ".", "Resources", fileName };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}