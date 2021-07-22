// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Extensions.Search
{
    public class SearchQuery
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("queryText")]
        public string QueryText { get; set; }

        [JsonProperty("dataset")]
        public string Dataset { get; set; }

        [JsonProperty("queryOptions")]
        public QueryOptions QueryOptions { get; set; }

        [JsonProperty("context")]
        public object Context{ get; set; }
    }
}
