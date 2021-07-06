// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Dialogs.Debugging.Source;

namespace AdaptiveBotSample
{
    public class Bot : ActivityHandler
    {
        private DialogSet _dialogs;
        private AdaptiveDialog rootDialog;
        private readonly ResourceExplorer resourceExplorer;

        public Bot(ConversationState conversationState, ResourceExplorer resourceExplorer, Source.IRegistry registry = null)
        {
            _dialogs = new DialogSet(conversationState.CreateProperty<DialogState>("DialogState"));

            this.resourceExplorer = resourceExplorer;
            registry = registry ?? NullRegistry.Instance;

            // auto reload dialogs when file changes
            this.resourceExplorer.Changed += (paths) =>
            {
                if (paths.Any(p => Path.GetExtension(p) == ".dialog"))
                {
                    Task.Run(() => this.LoadRootDialogAsync(registry));
                }
            };

            LoadRootDialogAsync(registry);
        }


        private void LoadRootDialogAsync(IRegistry registry)
        {
            System.Diagnostics.Trace.TraceInformation("Loading resources...");
            var rootFile = resourceExplorer.GetResource(@"ToDoBot.main.dialog");
            //var rootFile = resourceExplorer.GetResource("ToDoLuisBot.main.dialog");
            //var rootFile = resourceExplorer.GetResource("NoMatchRule.main.dialog");
            //var rootFile = resourceExplorer.GetResource("EndTurn.main.dialog");
            //var rootFile = resourceExplorer.GetResource("IfCondition.main.dialog");
            //var rootFile = resourceExplorer.GetResource("TextInput.main.dialog");
            //var rootFile = resourceExplorer.GetResource("WelcomeRule.main.dialog");
            //var rootFile = resourceExplorer.GetResource("DoSteps.main.dialog");
            //var rootFile = resourceExplorer.GetResource("BeginDialog.main.dialog");
            //var rootFile = resourceExplorer.GetResource("ExternalLanguage.main.dialog");
            //var rootFile = resourceExplorer.GetResource("CustomStep.dialog");

            rootDialog = DeclarativeTypeLoader.Load<AdaptiveDialog>(rootFile, resourceExplorer, registry);
            _dialogs.Add(rootDialog);

            System.Diagnostics.Trace.TraceInformation("Done loading resources.");
        }

        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await rootDialog.OnTurnAsync(turnContext, null, cancellationToken).ConfigureAwait(false);
        }
    }
}
