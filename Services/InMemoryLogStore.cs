using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Services;

/// <summary>
/// Stores Dreamine log entries in memory.
/// </summary>
public sealed class InMemoryLogStore : IDreamineLogStore, IDreamineLogSink
{
    private readonly object _syncRoot = new();
    private readonly List<DreamineLogEntry> _entries = new();
    private readonly int _capacity;

    /// <inheritdoc />
    public event EventHandler<DreamineLogEntry>? LogAdded;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryLogStore"/> class.
    /// </summary>
    public InMemoryLogStore()
        : this(1000)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryLogStore"/> class.
    /// </summary>
    /// <param name="capacity">The maximum number of log entries to keep.</param>
    public InMemoryLogStore(int capacity)
    {
        _capacity = capacity <= 0 ? 1000 : capacity;
    }

    /// <inheritdoc />
    public IReadOnlyList<DreamineLogEntry> GetEntries()
    {
        lock (_syncRoot)
        {
            return _entries.ToArray();
        }
    }

    /// <inheritdoc />
    public void Add(DreamineLogEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        lock (_syncRoot)
        {
            _entries.Add(entry);

            while (_entries.Count > _capacity)
            {
                _entries.RemoveAt(0);
            }
        }

        LogAdded?.Invoke(this, entry);
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_syncRoot)
        {
            _entries.Clear();
        }
    }

    /// <inheritdoc />
    public void Write(DreamineLogEntry entry)
    {
        Add(entry);
    }
}