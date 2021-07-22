// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using Microsoft.Bot.Extensions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.AdaptiveCards
{
    public class AdaptiveCardActionException : Exception
    {
        AdaptiveCardActionException(AdaptiveCardInvokeResponse response)
        {
            Response = response;
        }

        public AdaptiveCardInvokeResponse Response { get; private set; }

        public static void ErrorResponse(HttpStatusCode statusCode, string code, string message)
        {
            throw new AdaptiveCardActionException(new AdaptiveCardInvokeResponse()
            {
                StatusCode = (int)statusCode,
                Type = Constants.Error,
                Value = new Error()
                {
                    Code = code,
                    Message = message
                }
            });
        }

        public static void ActionNotSupported(string action)
        {
            ErrorResponse(HttpStatusCode.BadRequest, "NotSupported", $"The action '{action}'is not supported.");
        }

        public static void VerbNotSupported(string verb)
        {
            ErrorResponse(HttpStatusCode.BadRequest, "NotSupported", $"The verb '{verb}'is not supported.");
        }

        public static void BadRequest(string message)
        {
            ErrorResponse(HttpStatusCode.BadRequest, "BadRequest", message);
        }
    }
}
