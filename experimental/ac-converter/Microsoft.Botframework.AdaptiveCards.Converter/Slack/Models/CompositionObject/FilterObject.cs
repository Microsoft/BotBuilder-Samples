﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Botframework.AdaptiveCards.Converter.Slack.Models
{
    public class FilterObject
    {
        public string[] include { get; set; }
        public bool exclude_external_shared_channels { get; set; } = false;
        public bool exclude_bot_users { get; set; } = false;
    }
}
