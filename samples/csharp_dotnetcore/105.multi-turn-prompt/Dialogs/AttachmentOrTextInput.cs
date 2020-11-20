// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Custom AttachmentInput to allow the user to not provide an attachment,
    /// by sending 'skip' or any text.
    /// </summary>
    public class AttachmentOrTextInput : AttachmentInput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentOrTextInput"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public AttachmentOrTextInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine) { }

        /// <summary>
        /// Called when input has been received.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>InputState which reflects whether input was recognized as valid or not.</returns>
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            var input = dc.State.GetValue<List<Attachment>>(VALUE_PROPERTY);
            var first = input.Count > 0 ? input[0] : null;

            // NOTE: this custom AttachmentInput allows for no attachment.
            // If no Attachment is provided, 'first' will be null.
            // This could be updated to set the value property to dc.Context.Activity.Text, and
            // validate the text within a validator.  This sample just ignores Text input.

            //if (first == null || (string.IsNullOrEmpty(first.ContentUrl) && first.Content == null))
            //{
            //    return Task.FromResult(InputState.Unrecognized);
            //}

            switch (this.OutputFormat.GetValue(dc.State))
            {
                case AttachmentOutputFormat.All:
                    dc.State.SetValue(VALUE_PROPERTY, input);
                    break;
                case AttachmentOutputFormat.First:
                    dc.State.SetValue(VALUE_PROPERTY, first);
                    break;
            }

            return Task.FromResult(InputState.Valid);
        }
    }
}
