// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.RootBot
{
    /// <summary>
    /// A <see cref="SkillConversationIdFactory"/> that uses <see cref="IStorage"/> to store
    /// and retrieve <see cref="SkillConversationReference"/> instances.
    /// </summary>
    public class SkillConversationIdFactory : SkillConversationIdFactoryBase
    {
        private readonly IStorage _storage;

        public SkillConversationIdFactory(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public override async Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Create the storage key based on the SkillConversationIdFactoryOptions.
            var conversationReference = options.Activity.GetConversationReference();
            var skillConversationId = $"{conversationReference.Conversation.Id}-{options.BotFrameworkSkill.Id}-{conversationReference.ChannelId}-skillconvo";

            // Create the SkillConversationReference instance.
            var skillConversationReference = new SkillConversationReference
            {
                ConversationReference = conversationReference,
                OAuthScope = options.FromBotOAuthScope
            };

            // Store the SkillConversationReference using the skillConversationId as a key.
            var skillConversationInfo = new Dictionary<string, object> { { skillConversationId, JObject.FromObject(skillConversationReference) } };
            await _storage.WriteAsync(skillConversationInfo, cancellationToken).ConfigureAwait(false);

            // Return the generated skillConversationId (that will be also used as the conversation ID to call the skill).
            return skillConversationId;
        }

        public override async Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(skillConversationId))
            {
                throw new ArgumentNullException(nameof(skillConversationId));
            }

            // Get the SkillConversationReference from storage for the given skillConversationId.
            var skillConversationInfo = await _storage.ReadAsync(new[] { skillConversationId }, cancellationToken).ConfigureAwait(false);
            if (skillConversationInfo.Any())
            {
                var conversationInfo = ((JObject)skillConversationInfo[skillConversationId]).ToObject<SkillConversationReference>();
                return conversationInfo;
            }

            return null;
        }

        public override async Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            // Delete the SkillConversationReference from storage.
            await _storage.DeleteAsync(new[] { skillConversationId }, cancellationToken).ConfigureAwait(false);
        }
    }
}
