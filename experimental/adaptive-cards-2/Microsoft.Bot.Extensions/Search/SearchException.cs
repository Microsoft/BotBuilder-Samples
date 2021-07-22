// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Extensions.Search
{
    public class SearchException : Exception
    {
        SearchException(SearchInvokeResponse response)
        {
            Response = response;
        }

        public SearchInvokeResponse Response { get; private set; }

        public static Exception ErrorResponse(HttpStatusCode statusCode, string code, string message)
        {
            return new SearchException(new SearchInvokeResponse()
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

        public static Exception BadRequest(string message)
        {
            return ErrorResponse(HttpStatusCode.BadRequest, "BadRequest", message);
        }

        public static Exception DatasetNotSupported(string dataset)
        {
            return ErrorResponse(HttpStatusCode.BadRequest, "BadRequest", $"The dataset '{dataset}' is not supported.");
        }
    }
}
