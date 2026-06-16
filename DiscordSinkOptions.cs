namespace Serilog.Sinks.Discord
{
    public class DiscordSinkOptions
    {
        /// <summary>
        /// Serilog property names to include as embed fields. When empty, no properties are added.
        /// Example: ["application", "component", "environment"]
        /// </summary>
        public string[] PropertyFields { get; set; } = new string[0];

        /// <summary>
        /// Include the SourceContext property (logger/class name) as an embed field.
        /// </summary>
        public bool IncludeSourceContext { get; set; } = false;

        /// <summary>
        /// Walk the inner exception chain and include all messages as a single "Caused by" field.
        /// </summary>
        public bool IncludeInnerExceptions { get; set; } = false;

        /// <summary>
        /// Include the rendered Serilog log message alongside the exception details.
        /// Has no effect when the log event has no exception.
        /// </summary>
        public bool IncludeLogMessage { get; set; } = false;
    }
}
