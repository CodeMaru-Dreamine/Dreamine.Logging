using System;
using System.Collections.Generic;
using System.Linq;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Services
{
    /// <summary>
    /// Stores Dreamine log entries in memory using a bounded ring buffer.
    /// </summary>
    /// <remarks>
    /// Backed by a <see cref="Queue{T}"/> so that capacity enforcement is
    /// O(1) per insert (no <c>List.RemoveAt(0)</c> shifting cost). Thread-safe.
    /// </remarks>
    public sealed class InMemoryLogStore : IDreamineLogStore, IDreamineLogSink
    {
        private readonly object _syncRoot = new();
        private readonly Queue<DreamineLogEntry> _entries;
        private readonly int _capacity;

        /// <inheritdoc />
        public event EventHandler<DreamineLogEntry>? LogAdded;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLogStore"/> class
        /// with the default capacity of 1000 entries.
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
            _entries = new Queue<DreamineLogEntry>(_capacity);
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
            ArgumentNullException.ThrowIfNull(entry);

            lock (_syncRoot)
            {
                _entries.Enqueue(entry);

                while (_entries.Count > _capacity)
                {
                    _entries.Dequeue();
                }
            }

            // Fire outside the lock to avoid reentrancy/deadlock if a handler
            // calls back into the store.
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
}
