using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// Defines a formatter that converts a Dreamine log entry to text.
/// </summary>
public interface IDreamineLogFormatter
{
    /// <summary>
    /// Formats the specified log entry.
    /// </summary>
    /// <param name="entry">The log entry to format.</param>
    /// <returns>The formatted log text.</returns>
    string Format(DreamineLogEntry entry);
}