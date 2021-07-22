// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Extensions.Search
{
    public class DisplayLayout
    {
        [JsonProperty("layoutId")]
        public string LayoutId { get; set; }

        [JsonProperty("layoutBody")]
        public string LayoutBody { get; set; }
    }
}
