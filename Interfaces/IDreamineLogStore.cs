using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// Defines a readable log store used by UI or diagnostics.
/// </summary>
public interface IDreamineLogStore
{
    /// <summary>
    /// Occurs when a log entry is added.
    /// </summary>
    event EventHandler<DreamineLogEntry>? LogAdded;

    /// <summary>
    /// Gets all stored log entries.
    /// </summary>
    /// <returns>The stored log entries.</returns>
    IReadOnlyList<DreamineLogEntry> GetEntries();

    /// <summary>
    /// Adds a log entry.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    void Add(DreamineLogEntry entry);

    /// <summary>
    /// Clears all stored log entries.
    /// </summary>
    void Clear();
}