// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Extensions.Search
{
    public class SearchResults
    {
        [JsonProperty("totalResultCount")]
        public int TotalResultCount { get; set; }

        [JsonProperty("moreResultsAvailable")]
        public bool MoreResultsAvailabile { get; set; }

        [JsonProperty("results")]
        public IEnumerable<SearchResult> Results { get; set; }

        [JsonProperty("displayLayouts")]
        public IEnumerable<DisplayLayout> DisplayLayouts { get; set; }
    }
}
