using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Botframework.AdaptiveCards.Converter.Telegram
{
    public class InputFile
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("mediaType")]
        public string MediaType { get; set; }
    }

    public class TelegramMethod
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; }

        [JsonIgnore]
        public bool CheckAllowed;

        public static TelegramMethod CreateAudioMessage(string caption, string url, string mediaType, string keyboard = null)
        {
            var tmethod = new TelegramMethod()
            {
                Method = "sendAudio",
                Parameters = new Dictionary<string, object>()
                    {
                        { "caption", caption },
                        {
                            "audio", new InputFile()
                            {
                                Url = url,
                                MediaType = mediaType
                            }
                        },
                        { "reply_markup", keyboard }
                    }
            };
            return tmethod;
        }

        public static TelegramMethod CreateVideoMessage(string caption, string url, string mediaType, string keyboard = null)
        {
            var tmethod = new TelegramMethod()
            {
                Method = "sendVideo",
                Parameters = new Dictionary<string, object>()
                    {
                        { "caption", caption },
                        {
                            "video", new InputFile()
                            {
                                Url = url,
                                MediaType = mediaType
                            }
                        },
                        { "reply_markup", keyboard }
                    }
            };
            return tmethod;
        }
    }
}
