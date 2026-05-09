using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Sinks
{
    /// <summary>
    /// Decorates a synchronous log sink with a bounded background queue.
    /// </summary>
    /// <remarks>
    /// Producers (any thread calling <see cref="Write"/>) only enqueue; a single
    /// dedicated worker drains the queue and forwards entries to the inner sink.
    /// Designed to keep logging non-blocking on caller threads and to bound
    /// memory usage. When the queue is full the oldest pending entry is dropped
    /// instead of blocking the caller, so logging cannot stall the application.
    /// The number of dropped entries is observable via <see cref="DroppedCount"/>.
    /// </remarks>
    public sealed class AsyncQueueSink : IDreamineLogSink, IAsyncDisposable, IDisposable
    {
        private readonly IDreamineLogSink _inner;
        private readonly Channel<DreamineLogEntry> _channel;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _workerTask;
        private readonly int _drainBatchSize;
        private long _droppedCount;
        private int _disposed;

        /// <summary>
        /// Gets the number of entries that were dropped because the queue was full.
        /// </summary>
        public long DroppedCount => Interlocked.Read(ref _droppedCount);

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncQueueSink"/> class.
        /// </summary>
        /// <param name="inner">The synchronous sink to forward entries to.</param>
        /// <param name="capacity">The maximum number of pending entries. Default is 8192.</param>
        /// <param name="drainBatchSize">
        /// Maximum number of entries the worker drains per inner-sink dispatch loop.
        /// Larger values reduce per-entry overhead. Default is 256.
        /// </param>
        public AsyncQueueSink(
            IDreamineLogSink inner,
            int capacity = 8192,
            int drainBatchSize = 256)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));

            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (drainBatchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(drainBatchSize));
            }

            _drainBatchSize = drainBatchSize;

            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            };

            _channel = Channel.CreateBounded<DreamineLogEntry>(options);

            _workerTask = Task.Factory.StartNew(
                RunAsync,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default).Unwrap();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Non-blocking. Safe to call from any thread including high-frequency
        /// worker loops. If the queue is full the oldest entry is dropped.
        /// </remarks>
        public void Write(DreamineLogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            if (Volatile.Read(ref _disposed) != 0)
            {
                return;
            }

            // BoundedChannelFullMode.DropOldest guarantees TryWrite always succeeds
            // unless the channel is completed.
            if (!_channel.Writer.TryWrite(entry))
            {
                Interlocked.Increment(ref _droppedCount);
            }
        }

        private async Task RunAsync()
        {
            var reader = _channel.Reader;
            var batch = new List<DreamineLogEntry>(_drainBatchSize);

            try
            {
                while (await reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    batch.Clear();

                    while (batch.Count < _drainBatchSize && reader.TryRead(out var entry))
                    {
                        batch.Add(entry);
                    }

                    foreach (var item in batch)
                    {
                        try
                        {
                            _inner.Write(item);
                        }
                        catch
                        {
                            // Logging failure must not terminate the application.
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
            catch
            {
                // Defensive: never let the worker crash silently kill the channel.
            }
        }

        /// <summary>
        /// Stops accepting new entries and waits for pending entries to drain.
        /// </summary>
        /// <param name="timeout">Maximum time to wait for the worker to finish.</param>
        public async Task ShutdownAsync(TimeSpan timeout)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _channel.Writer.TryComplete();

            using var timeoutCts = new CancellationTokenSource(timeout);
            try
            {
                await _workerTask.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _cts.Cancel();
                try
                {
                    await _workerTask.ConfigureAwait(false);
                }
                catch
                {
                    // Suppress shutdown errors.
                }
            }

            // Best-effort dispose on the inner sink so file handles are closed.
            if (_inner is IAsyncDisposable asyncDisposable)
            {
                try
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Suppress on shutdown.
                }
            }
            else if (_inner is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // Suppress on shutdown.
                }
            }

            _cts.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await ShutdownAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                ShutdownAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
            }
            catch
            {
                // Suppress on dispose.
            }
        }
    }
}
