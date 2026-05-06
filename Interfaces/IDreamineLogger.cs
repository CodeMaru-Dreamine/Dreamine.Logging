using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// Defines the main logging API used by Dreamine components.
/// </summary>
public interface IDreamineLogger
{
    /// <summary>
    /// Writes a trace log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    void Trace(string message);

    /// <summary>
    /// Writes a debug log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    void Debug(string message);

    /// <summary>
    /// Writes an information log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    void Info(string message);

    /// <summary>
    /// Writes a warning log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    void Warning(string message);

    /// <summary>
    /// Writes an error log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    void Error(string message);

    /// <summary>
    /// Writes an error log message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message.</param>
    void Error(Exception exception, string message);

    /// <summary>
    /// Writes a fatal log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    void Fatal(string message);

    /// <summary>
    /// Writes a fatal log message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message.</param>
    void Fatal(Exception exception, string message);

    /// <summary>
    /// Writes a log entry directly.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    void Write(DreamineLogEntry entry);
}