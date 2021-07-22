using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public class CardResource
    {
        private readonly string _filePath;

        public CardResource(string filePath)
        {
            _filePath = filePath;
        }

        public string AsJson()
        {
            return File.ReadAllText(_filePath);
        }

        public string AsJson<T>(T data)
        {
            var templateJson = AsJson();
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJson);
            return template.Expand(data, o => string.Empty);
        }

        public object AsJObject()
        {
            return JsonConvert.DeserializeObject(AsJson());
        }

        public object AsJObject<T>(T data)
        {
            return JsonConvert.DeserializeObject(AsJson(data));
        }

        public Attachment AsAttachment()
        {
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(AsJson()),
            };
        }
        public Attachment AsAttachment<T>(T data)
        {
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(AsJson(data)),
            };
        }
    }
}
