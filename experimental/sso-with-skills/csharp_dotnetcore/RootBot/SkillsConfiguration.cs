// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.RootBot
{
    /// <summary>
    /// A helper class that loads Skills information from configuration.
    /// </summary>
    public class SkillsConfiguration
    {
        public SkillsConfiguration(IConfiguration configuration)
        {
            var section = configuration?.GetSection("BotFrameworkSkills");
            var skills = section?.Get<BotFrameworkSkill[]>();
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    Skills.Add(skill.Id, skill);
                }
            }

            var skillHostEndpoint = configuration?.GetValue<string>(nameof(SkillHostEndpoint));
            if (!string.IsNullOrWhiteSpace(skillHostEndpoint))
            {
                SkillHostEndpoint = new Uri(skillHostEndpoint);
            }
        }

        public Uri SkillHostEndpoint { get; }

        public Dictionary<string, BotFrameworkSkill> Skills { get; } = new Dictionary<string, BotFrameworkSkill>();
    }
}
