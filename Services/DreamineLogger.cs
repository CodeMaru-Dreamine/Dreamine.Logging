using System;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Services;

/// <summary>
/// Provides the default Dreamine logger implementation.
/// </summary>
public sealed class DreamineLogger : IDreamineLogger
{
    private readonly IReadOnlyList<IDreamineLogSink> _sinks;
    private readonly DreamineLogLevel _minimumLevel;
    private readonly string _category;

    /// <summary>
    /// Initializes a new instance of the <see cref="DreamineLogger"/> class.
    /// </summary>
    /// <param name="sink">The log sink.</param>
    public DreamineLogger(IDreamineLogSink sink)
        : this(new[] { sink }, DreamineLogLevel.Trace, "Dreamine")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DreamineLogger"/> class.
    /// </summary>
    /// <param name="sink">The log sink.</param>
    /// <param name="category">The log category.</param>
    public DreamineLogger(IDreamineLogSink sink, string category)
        : this(new[] { sink }, DreamineLogLevel.Trace, category)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DreamineLogger"/> class.
    /// </summary>
    /// <param name="sinks">The log sinks.</param>
    /// <param name="minimumLevel">The minimum log level.</param>
    /// <param name="category">The log category.</param>
    public DreamineLogger(
        IEnumerable<IDreamineLogSink> sinks,
        DreamineLogLevel minimumLevel = DreamineLogLevel.Trace,
        string category = "Dreamine")
    {
        if (sinks is null)
        {
            throw new ArgumentNullException(nameof(sinks));
        }

        _sinks = sinks.ToArray();

        if (_sinks.Count == 0)
        {
            throw new ArgumentException("At least one log sink is required.", nameof(sinks));
        }

        _minimumLevel = minimumLevel;
        _category = string.IsNullOrWhiteSpace(category) ? "Dreamine" : category;
    }

    /// <inheritdoc />
    public void Trace(string message) => Write(DreamineLogLevel.Trace, message, null);

    /// <inheritdoc />
    public void Debug(string message) => Write(DreamineLogLevel.Debug, message, null);

    /// <inheritdoc />
    public void Info(string message) => Write(DreamineLogLevel.Info, message, null);

    /// <inheritdoc />
    public void Warning(string message) => Write(DreamineLogLevel.Warning, message, null);

    /// <inheritdoc />
    public void Error(string message) => Write(DreamineLogLevel.Error, message, null);

    /// <inheritdoc />
    public void Error(Exception exception, string message) => Write(DreamineLogLevel.Error, message, exception);

    /// <inheritdoc />
    public void Fatal(string message) => Write(DreamineLogLevel.Fatal, message, null);

    /// <inheritdoc />
    public void Fatal(Exception exception, string message) => Write(DreamineLogLevel.Fatal, message, exception);

    /// <inheritdoc />
    public void Write(DreamineLogEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (entry.Level < _minimumLevel)
        {
            return;
        }

        foreach (var sink in _sinks)
        {
            try
            {
                sink.Write(entry);
            }
            catch
            {
                // Logging failure must not terminate the application.
            }
        }
    }

    private void Write(DreamineLogLevel level, string message, Exception? exception)
    {
        var entry = new DreamineLogEntry(
            DateTimeOffset.Now,
            level,
            _category,
            message,
            exception,
            Environment.CurrentManagedThreadId);

        Write(entry);
    }
}