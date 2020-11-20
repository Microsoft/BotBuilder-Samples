// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of dialog. The type parameter allows multiple different bots
    // to run at different endpoints within the same project. To do so, define multiple distinct controller types,
    // each with a dependency on a distinct IBot type. This allows ASP dependency injection to glue everything together without ambiguity.
    // The dialog manager has access to user state and conversation state from the turn context, and
    // it will save state changes before exiting.
    public class DialogBot<T> : IBot
        where T : Dialog
    {
        private readonly DialogManager DialogManager;
        protected readonly ILogger Logger;

        public DialogBot(T rootDialog, ILogger<DialogBot<T>> logger)
        {
            Logger = logger;

            DialogManager = new DialogManager(rootDialog);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Running dialog with Activity.");
            
            await DialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
