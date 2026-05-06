using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// Defines a log output target.
/// </summary>
public interface IDreamineLogSink
{
    /// <summary>
    /// Writes a log entry to the sink.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    void Write(DreamineLogEntry entry);
}