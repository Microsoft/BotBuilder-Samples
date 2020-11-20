// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class InterruptionsBot<T> : ActivityHandler 
        where T : Dialog
    {
        private readonly ILogger logger;
        private DialogManager dialogManager;

        public InterruptionsBot(ConversationState conversationState, UserState userState, T dialog, ILogger<InterruptionsBot<T>> logger)
        {
            this.logger = logger;
            dialogManager = new DialogManager(dialog);
            dialogManager.ConversationState = conversationState;
            dialogManager.UserState = userState;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.logger.LogInformation("Running dialog with Activity.");
            await dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
