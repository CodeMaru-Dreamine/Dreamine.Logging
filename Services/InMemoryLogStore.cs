using System;
using System.Collections.Generic;
using System.Linq;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Services
{
    /// <summary>
    /// \if KO
    /// <para>제한된 링 버퍼를 사용해 Dreamine 로그 항목을 메모리에 저장합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores Dreamine log entries in memory using a bounded ring buffer.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para><see cref="Queue{T}"/>를 사용해 삽입당 O(1)로 용량을 제한하며 스레드로부터 안전합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses a <see cref="Queue{T}"/> for O(1) capacity enforcement per insertion and is thread-safe.</para>
    /// \endif
    /// </remarks>
    public sealed class InMemoryLogStore : IDreamineLogStore, IDreamineLogSink
    {
        /// <summary>
        /// \if KO
        /// <para>sync Root 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the sync root value.</para>
        /// \endif
        /// </summary>
        private readonly object _syncRoot = new();
        /// <summary>
        /// \if KO
        /// <para>entries 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the entries value.</para>
        /// \endif
        /// </summary>
        private readonly Queue<DreamineLogEntry> _entries;
        /// <summary>
        /// \if KO
        /// <para>capacity 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the capacity value.</para>
        /// \endif
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        /// \if KO
        /// <para>로그 항목이 추가될 때 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Occurs when a log entry is added.</para>
        /// \endif
        /// </summary>
        public event EventHandler<DreamineLogEntry>? LogAdded;

        /// <summary>
        /// \if KO
        /// <para>기본 용량 1000으로 <see cref="InMemoryLogStore"/>를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes <see cref="InMemoryLogStore"/> with the default capacity of 1000 entries.</para>
        /// \endif
        /// </summary>
        public InMemoryLogStore()
            : this(1000)
        {
        }

        /// <summary>
        /// \if KO
        /// <para>지정한 용량으로 <see cref="T:Dreamine.Logging.Services.InMemoryLogStore" />를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes <see cref="T:Dreamine.Logging.Services.InMemoryLogStore" /> with the specified capacity.</para>
        /// \endif
        /// </summary>
        /// <param name="capacity">
        /// \if KO
        /// <para>유지할 최대 항목 수이며 0 이하면 1000을 사용합니다.</para>
        /// \endif
        /// \if EN
        /// <para>The maximum entry count; 1000 is used when non-positive.</para>
        /// \endif
        /// </param>
        public InMemoryLogStore(int capacity)
        {
            _capacity = capacity <= 0 ? 1000 : capacity;
            _entries = new Queue<DreamineLogEntry>(_capacity);
        }

        /// <summary>
        /// \if KO
        /// <para>저장된 로그 항목의 스냅샷을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets a snapshot of stored log entries.</para>
        /// \endif
        /// </summary>
        /// <returns>
        /// \if KO
        /// <para>저장 순서의 로그 항목 목록입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The log entries in storage order.</para>
        /// \endif
        /// </returns>
        public IReadOnlyList<DreamineLogEntry> GetEntries()
        {
            lock (_syncRoot)
            {
                return _entries.ToArray();
            }
        }

        /// <summary>
        /// \if KO
        /// <para>로그 항목을 추가하고 용량을 초과한 가장 오래된 항목을 제거합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Adds an entry and removes the oldest entries exceeding capacity.</para>
        /// \endif
        /// </summary>
        /// <param name="entry">
        /// \if KO
        /// <para>추가할 항목입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The entry to add.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="entry"/>가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="entry"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
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

        /// <summary>
        /// \if KO
        /// <para>저장된 모든 로그 항목을 지웁니다.</para>
        /// \endif
        /// \if EN
        /// <para>Clears all stored log entries.</para>
        /// \endif
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _entries.Clear();
            }
        }

        /// <summary>
        /// \if KO
        /// <para>로그 출력 계약을 통해 항목을 저장소에 추가합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Adds an entry to the store through the log-sink contract.</para>
        /// \endif
        /// </summary>
        /// <param name="entry">
        /// \if KO
        /// <para>기록할 항목입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The entry to write.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="entry"/>가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="entry"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        public void Write(DreamineLogEntry entry)
        {
            Add(entry);
        }
    }
}
