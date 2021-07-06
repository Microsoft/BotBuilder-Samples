using Jurassic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdaptiveBotSample
{
    /// <summary>
    /// Custom command which takes the input property and performs a calculation, returning it back to the caller
    /// </summary>
    public class CalculateDogYears : DialogCommand
    {

        [JsonConstructor]
        public CalculateDogYears([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }


        public string InputProperty { get; set; }
        
        protected override Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var value = dc.State.GetValue<int>(this.InputProperty);
            var result = (float)value / 7;
            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
