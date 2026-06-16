using Discord;
using Discord.Webhook;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Text;

namespace Serilog.Sinks.Discord
{
    public class DiscordSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly UInt64 _webhookId;
        private readonly string _webhookToken;
        private readonly LogEventLevel _restrictedToMinimumLevel;
        private readonly DiscordSinkOptions _options;

        public DiscordSink(
            IFormatProvider formatProvider,
            UInt64 webhookId,
            string webhookToken,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information,
            DiscordSinkOptions options = null)
        {
            _formatProvider = formatProvider;
            _webhookId = webhookId;
            _webhookToken = webhookToken;
            _restrictedToMinimumLevel = restrictedToMinimumLevel;
            _options = options;
        }

        public void Emit(LogEvent logEvent)
        {
            SendMessage(logEvent);
        }

        private void SendMessage(LogEvent logEvent)
        {
            if (!ShouldLogMessage(_restrictedToMinimumLevel, logEvent.Level))
                return;

            var embedBuilder = new EmbedBuilder();
            var webHook = new DiscordWebhookClient(_webhookId, _webhookToken);

            try
            {
                SpecifyEmbedLevel(logEvent.Level, embedBuilder);

                if (logEvent.Exception != null)
                {
                    embedBuilder.AddField("Type:", $"```{logEvent.Exception.GetType().FullName}```");

                    var exMessage = FormatMessage(logEvent.Exception.Message, 1000);
                    if (exMessage != null)
                        embedBuilder.AddField("Message:", exMessage);

                    if (_options != null && _options.IncludeLogMessage)
                    {
                        var logMessage = FormatMessage(logEvent.RenderMessage(_formatProvider), 1000);
                        if (logMessage != null)
                            embedBuilder.AddField("Log Message:", logMessage);
                    }

                    if (_options != null && _options.IncludeInnerExceptions && logEvent.Exception.InnerException != null)
                    {
                        var inner = FormatMessage(BuildInnerExceptionChain(logEvent.Exception.InnerException), 1000);
                        if (inner != null)
                            embedBuilder.AddField("Caused by:", inner);
                    }

                    if (_options != null && _options.IncludeSourceContext)
                    {
                        var source = GetProperty(logEvent, "SourceContext");
                        if (source != null)
                            embedBuilder.AddField("Source:", $"`{source}`");
                    }

                    if (_options != null && _options.PropertyFields != null)
                    {
                        foreach (var propName in _options.PropertyFields)
                        {
                            var val = GetProperty(logEvent, propName);
                            if (val != null)
                                embedBuilder.AddField(propName + ":", $"`{val}`", inline: true);
                        }
                    }

                    var stackTrace = FormatMessage(logEvent.Exception.StackTrace, 1000);
                    if (stackTrace != null)
                        embedBuilder.AddField("StackTrace:", stackTrace);
                }
                else
                {
                    var message = FormatMessage(logEvent.RenderMessage(_formatProvider), 1000);

                    if (_options != null && _options.IncludeSourceContext)
                    {
                        var source = GetProperty(logEvent, "SourceContext");
                        if (source != null)
                            embedBuilder.AddField("Source:", $"`{source}`");
                    }

                    if (_options != null && _options.PropertyFields != null)
                    {
                        foreach (var propName in _options.PropertyFields)
                        {
                            var val = GetProperty(logEvent, propName);
                            if (val != null)
                                embedBuilder.AddField(propName + ":", $"`{val}`", inline: true);
                        }
                    }

                    embedBuilder.Description = message;
                }

                webHook.SendMessageAsync(null, false, new Embed[] { embedBuilder.Build() })
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                webHook.SendMessageAsync(
                    $"ooo snap, {ex.Message}", false)
                    .GetAwaiter()
                    .GetResult();
            }
        }

        private static string GetProperty(LogEvent logEvent, string name)
        {
            if (logEvent.Properties.TryGetValue(name, out var val))
                return val.ToString().Trim('"');
            return null;
        }

        private static string BuildInnerExceptionChain(Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                if (sb.Length > 0)
                    sb.Append(" → ");
                sb.Append(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }

        private static void SpecifyEmbedLevel(LogEventLevel level, EmbedBuilder embedBuilder)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    embedBuilder.Title = ":loud_sound: Verbose";
                    embedBuilder.Color = Color.LightGrey;
                    break;
                case LogEventLevel.Debug:
                    embedBuilder.Title = ":mag: Debug";
                    embedBuilder.Color = Color.LightGrey;
                    break;
                case LogEventLevel.Information:
                    embedBuilder.Title = ":information_source: Information";
                    embedBuilder.Color = new Color(0, 186, 255);
                    break;
                case LogEventLevel.Warning:
                    embedBuilder.Title = ":warning: Warning";
                    embedBuilder.Color = new Color(255, 204, 0);
                    break;
                case LogEventLevel.Error:
                    embedBuilder.Title = ":x: Error";
                    embedBuilder.Color = new Color(255, 0, 0);
                    break;
                case LogEventLevel.Fatal:
                    embedBuilder.Title = ":skull_crossbones: Fatal";
                    embedBuilder.Color = Color.DarkRed;
                    break;
            }
        }

        public static string FormatMessage(string message, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            if (message.Length > maxLength)
                message = $"{message.Substring(0, maxLength)} ...";

            return $"```{message}```";
        }

        private static bool ShouldLogMessage(
            LogEventLevel minimumLogEventLevel,
            LogEventLevel messageLogEventLevel) =>
                (int)messageLogEventLevel >= (int)minimumLogEventLevel;
    }
}
