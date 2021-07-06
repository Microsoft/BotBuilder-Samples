using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace AdaptiveBotSample
{
    public class CSharpStep : DialogCommand
    {
        private static List<MetadataReference> refs = new List<MetadataReference>{
                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location)};

        private string script;
        private Script<object> compiledScript;

        /// <summary>
        /// CSHarp script bound to memory (user, conversation, dialog, turn)
        /// </summary>
        /// <example>
        /// Example: inline
        /// if (user.age > 18)
        ///     return dialog.lastResult;
        /// return null;
        /// 
        /// Example: filename -> step.csx
        /// dynamic DoStep(dynamic user, dynamic conversation, dynamic dialog, dynamic turn)
        /// {
        ///     var age = System.Convert.ToSingle(user["age"]);
        ///     conversation["cat"] = age / 5;
        ///     return age * 5;
        /// }
        /// </example>
        public string Script { get { return script; } set { LoadScript(value); } }

        [JsonConstructor]
        public CSharpStep([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Exception error = null;
            var result = await compiledScript.RunAsync((object)dc.State, (exception) =>
            {
                error = exception;
                return true;
            });
            if (error != null)
            {
                await dc.Context.SendActivityAsync(error.Message);
                return await dc.EndDialogAsync().ConfigureAwait(false);
            }
            return await dc.EndDialogAsync(result.ReturnValue, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{nameof(CSharpStep)}({this.script.GetHashCode()})";
        }

        private void LoadScript(string value)
        {
            StringBuilder sb = new StringBuilder();
            if (File.Exists(value))
            {
                this.script = File.ReadAllText(value);
                sb.AppendLine(script);
            }
            else
            {
                this.script = value;
                sb.AppendLine("dynamic DoStep(dynamic user, dynamic conversation, dynamic dialog, dynamic turn)");
                sb.AppendLine("{");
                sb.AppendLine(script);
                sb.AppendLine("}");
            }

            sb.AppendLine("return DoStep(User, Conversation, Dialog, Turn);");

            this.compiledScript = CSharpScript.Create(sb.ToString(),
                options: ScriptOptions.Default.AddReferences(refs)
                            .AddImports("System.Dynamic"),
                globalsType: typeof(DialogContextState));
        }
    }
}
