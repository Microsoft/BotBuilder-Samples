using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveCards;

namespace Microsoft.Botframework.AdaptiveCards.Converter.Telegram
{
    public class TelegramCardConverter
    {
        public async Task<List<TelegramMethod>> ToChannelData(List<AdaptiveCard> cards)
        {
            var telegramMethods = new List<TelegramMethod>();
            foreach (var card in cards)
            {
                telegramMethods.AddRange(await ConvertAdaptiveCard(card).ConfigureAwait(false));
            }

            return telegramMethods;
        }

        public async Task<List<TelegramMethod>> ToChannelData(AdaptiveCard card)
        {
            var telegramMethods = await ConvertAdaptiveCard(card).ConfigureAwait(false);
            return telegramMethods;
        }

        public async Task<List<TelegramMethod>> ConvertAdaptiveCard(AdaptiveCard card)
        {
            var telegramMethods = new List<TelegramMethod>();
            foreach (var element in card.Body)
            {
                telegramMethods.AddRange(await ConvertAdaptiveElement(element).ConfigureAwait(false));
            }

            return telegramMethods;
        }

        #region Card elements

        public async Task<List<TelegramMethod>> ConvertAdaptiveMedia(AdaptiveMedia media)
        {
            var telegramMethods = new List<TelegramMethod>();
            foreach (var mediaSource in media.Sources)
            {
                switch (mediaSource.MimeType)
                {
                    case "audio/basic":
                    case "audio/mid":
                    case "audio/mpeg":
                    case "audio/x-aiff":
                    case "audio/x-mpegurl":
                    case "audio/x-pn-realaudio":
                    case "audio/x-wav":
                        {
                            var telegramMethod = TelegramMethod.CreateAudioMessage(null, mediaSource.Url, mediaSource.MimeType);
                            telegramMethods.Add(telegramMethod);
                            break;
                        }

                    case "video/mp4":
                    case "video/mpeg":
                    case "video/quicktime":
                    case "video/x-la-asf":
                    case "video/x-ms-asf":
                    case "video/x-msvideo":
                    case "video/x-sgi-movie":
                        {
                            var telegramMethod = TelegramMethod.CreateVideoMessage(null, mediaSource.Url, mediaSource.MimeType);
                            telegramMethods.Add(telegramMethod);
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }

            return telegramMethods;
        }

        #endregion

        #region Containers

        /// <summary>
        /// </summary>
        /// <param name="adaptiveContainer">The Adaptive card "Container" Element.</param>
        /// <returns>The converted <see cref="BoxComponent"/></returns>
        private async Task<List<TelegramMethod>> ConvertAdaptiveContainer(AdaptiveContainer adaptiveContainer)
        {
            var telegramMethods = new List<TelegramMethod>();
            foreach (var element in adaptiveContainer.Items)
            {
                telegramMethods.AddRange(await ConvertAdaptiveElement(element).ConfigureAwait(false));
            }

            return telegramMethods;
        }

        /// <summary>
        /// Convert adaptive card column set <see cref="AdaptiveColumnSet"/> to slack blocks.
        /// </summary>
        /// <param name="columnSet">The column set need to convert.</param>
        private async Task<List<TelegramMethod>> ConvertColumnSet(AdaptiveColumnSet columnSet)
        {
            var telegramMethods = new List<TelegramMethod>();
            foreach (var column in columnSet.Columns)
            {
                telegramMethods.AddRange(await ConvertColumn(column).ConfigureAwait(false));
            }

            return telegramMethods;
        }

        /// <summary>
        /// Convert adaptive card Column <see cref="AdaptiveColumn"/> to slack blocks.
        /// </summary>
        /// <param name="column">The Column need to convert.</param>
        private async Task<List<TelegramMethod>> ConvertColumn(AdaptiveColumn column)
        {
            var telegramMethods = new List<TelegramMethod>();
            foreach (var element in column.Items)
            {
                telegramMethods.AddRange(await ConvertAdaptiveElement(element).ConfigureAwait(false));
            }

            return telegramMethods;
        }

        /// <summary>
        /// Convert general Adaptive card element to Slack block.
        /// </summary>
        /// <param name="adaptiveElement">The adaptive card element need to convert.</param>
        private async Task<List<TelegramMethod>> ConvertAdaptiveElement(AdaptiveElement adaptiveElement)
        {
            if (adaptiveElement is AdaptiveMedia media)
            {
                return await ConvertAdaptiveMedia(media).ConfigureAwait(false);
            }
            else if (adaptiveElement is AdaptiveContainer container)
            {
                return await ConvertAdaptiveContainer(container).ConfigureAwait(false);
            }
            else if (adaptiveElement is AdaptiveColumnSet columnSet)
            {
                return await ConvertColumnSet(columnSet).ConfigureAwait(false);
            }
            else
            {

                // throw new NotSupportedException($"Element type not support, type: {adaptiveElement.GetType().FullName}");
                return new List<TelegramMethod>();
            }
        }

        #endregion
    }
}
