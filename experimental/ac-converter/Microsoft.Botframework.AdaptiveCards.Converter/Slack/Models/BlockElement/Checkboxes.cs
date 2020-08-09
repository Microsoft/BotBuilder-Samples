﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Botframework.AdaptiveCards.Converter.Slack.Models
{
    public class Checkboxes : IBlockElement
    {
        public string type { get; set; } = "checkboxes";
        public string action_id { get; set; }
        public OptionObject[] options { get; set; }
        public OptionObject[] initial_options { get; set; }
        public ConfirmObject confirm { get; set; }
    }
}
