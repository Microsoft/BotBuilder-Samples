using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.AdaptiveCards
{
    public class AdaptiveCardOAuthResult
    {
        public Activity OriginalActivity { get; set; }

        public TokenResponse TokenResponse { get; set; }

        public AdaptiveCardInvokeResponse InvokeResponse { get; set; }
    }
}
