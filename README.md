# Dreamine.Logging

Dreamine.Logging provides the core logging infrastructure for Dreamine applications.
It defines logger abstractions, log entry models, in-memory log storage, text formatting, composite sinks, and daily text file output.

## Purpose

This package is designed to provide a lightweight and extensible logging pipeline that can be used by Dreamine-based applications without depending on WPF or any UI framework.

## Features

- `IDreamineLogger` abstraction
- Log level model with `Trace`, `Debug`, `Info`, `Warning`, `Error`, and `Fatal`
- `DreamineLogEntry` model for structured log data
- `InMemoryLogStore` for runtime diagnostics and UI integration
- `IDreamineLogSink` for pluggable log output targets
- `CompositeLogSink` for writing to multiple targets
- `DreamineTextLogFormatter` for plain text formatting
- `TextFileLogSink` for daily log file output

## Basic Architecture

```text
IDreamineLogger
  -> CompositeLogSink
     -> InMemoryLogStore
     -> TextFileLogSink
```

## Example Registration

```csharp
var logStore = new InMemoryLogStore();
var formatter = new DreamineTextLogFormatter();

var textFileSink = new TextFileLogSink(
    Path.Combine(AppContext.BaseDirectory, "Logs"),
    formatter);

var compositeSink = new CompositeLogSink(new IDreamineLogSink[]
{
    logStore,
    textFileSink
});

DMContainer.RegisterSingleton<IDreamineLogStore>(logStore);
DMContainer.RegisterSingleton<IDreamineLogSink>(compositeSink);
DMContainer.RegisterSingleton<IDreamineLogFormatter>(formatter);
DMContainer.RegisterSingleton<IDreamineLogger>(
    new DreamineLogger(compositeSink, "SampleSmart"));
```

## Log File Output

When `TextFileLogSink` is used, log files are written by date.

```text
Logs/yyyy-MM-dd.log
```

Example:

```text
[2026-05-05 23:00:22.718] [Info] [SampleSmart] [T1] PageLog requested.
```

## Thread Safety

`TextFileLogSink` uses an internal lock to serialize file writes when a single sink instance is shared.
For normal WPF applications, registering the sink once and reusing it through `DMContainer` is sufficient.

For high-volume or multi-process logging scenarios, an async queue-based dispatcher and external logging service should be considered.

## Recommended Usage

This package should remain UI-independent.
WPF-specific log panel components are provided by `Dreamine.Logging.Wpf`.

## Future Roadmap

- `DreamineLoggerOptions`
- `DMContainer.UseDreamineLogging(...)`
- Async queue-based log dispatcher
- File rolling policy
- Database sink
- External logging service sink
- Graceful flush on application shutdown

## License

MIT License
