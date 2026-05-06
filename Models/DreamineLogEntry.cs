using System;

namespace Dreamine.Logging.Models;

/// <summary>
/// Represents a single Dreamine log entry.
/// </summary>
public sealed class DreamineLogEntry
{
    /// <summary>
    /// Gets the time when the log entry was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the severity level of the log entry.
    /// </summary>
    public DreamineLogLevel Level { get; }

    /// <summary>
    /// Gets the log category.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets the log message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the exception associated with the log entry.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the managed thread id where the log entry was created.
    /// </summary>
    public int ThreadId { get; }

    /// <summary>
    /// Gets the formatted timestamp text.
    /// </summary>
    public string TimestampText => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

    /// <summary>
    /// Gets the display text used by simple UI views.
    /// </summary>
    public string DisplayText
    {
        get
        {
            var text = $"[{TimestampText}] [{Level}] [{Category}] [T{ThreadId}] {Message}";

            if (Exception is null)
            {
                return text;
            }

            return text + Environment.NewLine + Exception;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DreamineLogEntry"/> class.
    /// </summary>
    /// <param name="timestamp">The time when the log entry was created.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="category">The log category.</param>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception associated with the log entry.</param>
    /// <param name="threadId">The managed thread id.</param>
    public DreamineLogEntry(
        DateTimeOffset timestamp,
        DreamineLogLevel level,
        string category,
        string message,
        Exception? exception,
        int threadId)
    {
        Timestamp = timestamp;
        Level = level;
        Category = string.IsNullOrWhiteSpace(category) ? "Dreamine" : category;
        Message = message ?? string.Empty;
        Exception = exception;
        ThreadId = threadId;
    }
}