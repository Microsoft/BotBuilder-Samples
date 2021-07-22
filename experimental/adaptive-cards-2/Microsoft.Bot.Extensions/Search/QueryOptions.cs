using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Extensions.Search
{
    public class QueryOptions
    {
        [JsonProperty("skip")]
        public int? Skip { get; set; }

        [JsonProperty("top")]
        public int? Top { get; set; }
    }
}
