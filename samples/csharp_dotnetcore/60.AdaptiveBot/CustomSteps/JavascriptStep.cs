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
    public class JavascriptStep : DialogCommand
    {
        private ScriptEngine scriptEngine;
        private string script;

        /// <summary>
        /// Javascript bound to memory run function(user, conversation, dialog, turn)
        /// </summary>
        /// <example>
        /// example inline script:
        ///        if (user.age > 18)
        ///              return dialog.lastResult;
        ///          return null;
        /// Example file script.js:
        /// function doStep(user, conversation, dialog, turn) {
        ///    if (user.age)
        ///        return user.age* 7;
        ///    return 0;
        /// }
        /// </example>
        public string Script { get { return script; } set { LoadScript(value); } }

        [JsonConstructor]
        public JavascriptStep([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.scriptEngine = new ScriptEngine();

            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // map state into json
            dynamic payload = new JObject();
            payload.state = new JObject();
            payload.state.user = JObject.FromObject(dc.State.User);
            payload.state.conversation = JObject.FromObject(dc.State.Conversation);
            payload.state.dialog = JObject.FromObject(dc.State.Dialog);
            payload.state.turn = JObject.FromObject(dc.State.Turn);

            // payload.property = (this.Property != null) ? dc.GetValue<object>(this.Property) : null;
            string payloadJson = JsonConvert.SerializeObject(payload);
            var responseJson = scriptEngine.CallGlobalFunction<string>("callStep", payloadJson);

            if (!String.IsNullOrEmpty(responseJson))
            {
                dynamic response = JsonConvert.DeserializeObject(responseJson);
                payload.state.User = response.state.user;
                payload.state.Conversation = response.state.conversation;
                payload.state.Dialog = response.state.dialog;
                payload.state.Turn = response.state.turn;
                return dc.EndDialogAsync((object)response.result, cancellationToken: cancellationToken);
            }
            return dc.EndDialogAsync(cancellationToken: cancellationToken);
        }

        protected override string OnComputeId()
        {
            return $"{nameof(JavascriptStep)}({this.script.GetHashCode()})";
        }

        private void LoadScript(string value)
        {
            if (File.Exists(value))
            {
                this.script = File.ReadAllText(value);
            }
            else
            {
                this.script = value;
            }

            // define the function
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(script);
            sb.AppendLine(@"function callStep(payloadJson) { 
	                var payload = JSON.parse(payloadJson);

                    // run script
	                payload.result = doStep(payload.state.user, 
                        payload.state.conversation, 
                        payload.state.dialog, 
                        payload.state.turn);
	                
                    return JSON.stringify(payload, null, 4);
                }");

            scriptEngine.Evaluate(sb.ToString());
        }


    }
}
