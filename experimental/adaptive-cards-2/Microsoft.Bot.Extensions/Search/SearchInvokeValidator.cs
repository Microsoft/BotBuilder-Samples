// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Bot.Extensions.Search
{
    public class SearchInvokeValidator
    {
        public static bool IsSearchQuery(Activity activity)
        {
            return activity.Type == ActivityTypes.Invoke &&
                string.Equals(Constants.Search, activity.Name);
        }

        public static SearchQuery ValidateRequest(Activity activity)
        {
            SearchQuery request = null;

            if (activity.Value == null)
            {
                SearchException.BadRequest("Missing value property");
            }

            try
            {
                request = ((JObject)activity.Value).ToObject<SearchQuery>();
            }
            catch(Exception)
            {
                SearchException.BadRequest("Value property is not properly formed");
            }

            if (request.QueryText == null)
            {
                SearchException.BadRequest("Missing QueryText property");
            }

            return request;
        }
    }
}
