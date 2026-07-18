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
    /// \if KO
    /// <para>동기 로그 출력을 제한된 백그라운드 큐로 감쌉니다.</para>
    /// \endif
    /// \if EN
    /// <para>Decorates a synchronous log sink with a bounded background queue.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>호출자는 비차단으로 항목을 큐에 넣고 단일 워커가 내부 출력으로 전달합니다. 큐가 차면 가장 오래된 항목을 버립니다.</para>
    /// \endif
    /// \if EN
    /// <para>Callers enqueue without blocking while one worker forwards entries; the oldest entry is dropped when the queue is full.</para>
    /// \endif
    /// </remarks>
    public sealed class AsyncQueueSink : IDreamineLogSink, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// \if KO
        /// <para>inner 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the inner value.</para>
        /// \endif
        /// </summary>
        private readonly IDreamineLogSink _inner;
        /// <summary>
        /// \if KO
        /// <para>channel 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the channel value.</para>
        /// \endif
        /// </summary>
        private readonly Channel<DreamineLogEntry> _channel;
        /// <summary>
        /// \if KO
        /// <para>cts 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the cts value.</para>
        /// \endif
        /// </summary>
        private readonly CancellationTokenSource _cts = new();
        /// <summary>
        /// \if KO
        /// <para>worker Task 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the worker task value.</para>
        /// \endif
        /// </summary>
        private readonly Task _workerTask;
        /// <summary>
        /// \if KO
        /// <para>drain Batch Size 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the drain batch size value.</para>
        /// \endif
        /// </summary>
        private readonly int _drainBatchSize;
        /// <summary>
        /// \if KO
        /// <para>dropped Count 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the dropped count value.</para>
        /// \endif
        /// </summary>
        private long _droppedCount;
        /// <summary>
        /// \if KO
        /// <para>disposed 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the disposed value.</para>
        /// \endif
        /// </summary>
        private int _disposed;

        /// <summary>
        /// \if KO
        /// <para>큐가 가득 차서 버려진 항목 수를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the number of entries dropped because the queue was full.</para>
        /// \endif
        /// </summary>
        public long DroppedCount => Interlocked.Read(ref _droppedCount);

        /// <summary>
        /// \if KO
        /// <para>내부 출력과 큐 설정으로 <see cref="T:Dreamine.Logging.Sinks.AsyncQueueSink" />를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes <see cref="T:Dreamine.Logging.Sinks.AsyncQueueSink" /> with an inner sink and queue settings.</para>
        /// \endif
        /// </summary>
        /// <param name="inner">
        /// \if KO
        /// <para>항목을 전달할 동기 출력입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The synchronous sink that receives entries.</para>
        /// \endif
        /// </param>
        /// <param name="capacity">
        /// \if KO
        /// <para>최대 대기 항목 수입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The maximum number of pending entries.</para>
        /// \endif
        /// </param>
        /// <param name="drainBatchSize">
        /// \if KO
        /// <para>내부 출력과 큐 설정으로 <see cref="T:Dreamine.Logging.Sinks.AsyncQueueSink" />를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes <see cref="T:Dreamine.Logging.Sinks.AsyncQueueSink" /> with an inner sink and queue settings.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="inner"/>가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="inner"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// \if KO
        /// <para>용량 또는 배치 크기가 0 이하인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when the capacity or batch size is non-positive.</para>
        /// \endif
        /// </exception>
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

        /// <summary>
        /// \if KO
        /// <para>로그 항목을 비차단 방식으로 백그라운드 큐에 추가합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Enqueues a log entry on the background queue without blocking.</para>
        /// \endif
        /// </summary>
        /// <param name="entry">
        /// \if KO
        /// <para>추가할 로그 항목입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The log entry to enqueue.</para>
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
        /// <remarks>
        /// \if KO
        /// <para>로그 항목을 비차단 방식으로 백그라운드 큐에 추가합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Enqueues a log entry on the background queue without blocking.</para>
        /// \endif
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

        /// <summary>
        /// \if KO
        /// <para>큐를 배치 단위로 비우고 내부 출력에 전달하는 전용 워커를 실행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Runs the dedicated worker that drains the queue in batches into the inner sink.</para>
        /// \endif
        /// </summary>
        /// <returns>
        /// \if KO
        /// <para>워커 수명 작업입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The worker lifetime task.</para>
        /// \endif
        /// </returns>
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
        /// \if KO
        /// <para>새 항목 수락을 중지하고 대기 항목이 처리될 때까지 기다립니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stops accepting new entries and waits for pending entries to drain.</para>
        /// \endif
        /// </summary>
        /// <param name="timeout">
        /// \if KO
        /// <para>워커 종료를 기다릴 최대 시간입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The maximum time to wait for the worker.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>비동기 종료 작업입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The asynchronous shutdown operation.</para>
        /// \endif
        /// </returns>
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

        /// <summary>
        /// \if KO
        /// <para>기본 제한 시간으로 큐를 종료하고 리소스를 비동기 해제합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Shuts down the queue with the default timeout and releases resources asynchronously.</para>
        /// \endif
        /// </summary>
        /// <returns>
        /// \if KO
        /// <para>비동기 해제 작업입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The asynchronous disposal operation.</para>
        /// \endif
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            await ShutdownAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }

        /// <summary>
        /// \if KO
        /// <para>기본 제한 시간으로 큐를 동기 종료하고 오류를 억제합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Shuts down the queue synchronously with the default timeout while suppressing errors.</para>
        /// \endif
        /// </summary>
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
