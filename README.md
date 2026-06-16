# Menelabs.Serilog.Sinks.Discord

Serilog sink for Discord with rich embed support — structured fields, inner exception chain, source context, and custom Serilog property fields.

> Fork of [serilog-contrib/Serilog-Sinks-Discord](https://github.com/serilog-contrib/Serilog-Sinks-Discord) by Abolfazl Edgelolli, extended with richer embed output via `DiscordSinkOptions`.

---

## Getting started

### 1. Create a Discord webhook

Follow the [Discord webhook guide](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks) and copy the webhook URL:

```
https://discordapp.com/api/webhooks/{WebhookId}/{WebhookToken}
```

### 2. Install the NuGet package

```
dotnet add package Menelabs.Serilog.Sinks.Discord
```

### 3. Add Discord output

Basic usage (same as the original package):

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Discord(webhookId, webhookToken)
    .CreateLogger();
```

For async logging use [serilog-sinks-async](https://github.com/serilog/serilog-sinks-async):

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(a => a.Discord(webhookId, webhookToken))
    .Enrich.FromLogContext()
    .CreateLogger();
```

---

## Rich embeds with DiscordSinkOptions

Pass a `DiscordSinkOptions` instance to enable additional embed fields:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Discord(
        webhookId,
        webhookToken,
        restrictedToMinimumLevel: LogEventLevel.Error,
        options: new DiscordSinkOptions
        {
            IncludeSourceContext = true,
            IncludeInnerExceptions = true,
            IncludeLogMessage = true,
            PropertyFields = new[] { "application", "environment" }
        })
    .CreateLogger();
```

### Options

| Property | Default | Description |
|---|---|---|
| `IncludeSourceContext` | `false` | Adds the logger name (`SourceContext`) as an embed field |
| `IncludeInnerExceptions` | `false` | Walks the inner exception chain and adds all messages as a "Caused by" field |
| `IncludeLogMessage` | `false` | Adds the rendered Serilog log message alongside exception details |
| `PropertyFields` | `[]` | Serilog property names to include as embed fields (e.g. `["application", "environment"]`). When empty, no properties are added |

All options default to off — existing setups need no changes.

---

## Screenshots

![Example embed](/Screenshots/screenshot.png?raw=true)

![Example embed](/Screenshots/screenshot1.png?raw=true)
