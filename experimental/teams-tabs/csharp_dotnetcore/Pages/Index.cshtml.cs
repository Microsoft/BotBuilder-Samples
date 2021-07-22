// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IConfiguration config)
        {
            MicrosoftAppId = config["MicrosoftAppId"];
        }

        public string MicrosoftAppId { get; private set; }

        public void OnGet()
        {
        }
    }
}
