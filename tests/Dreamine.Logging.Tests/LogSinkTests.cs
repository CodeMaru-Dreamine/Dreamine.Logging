using Dreamine.Logging.Formatters;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;
using Dreamine.Logging.Options;
using Dreamine.Logging.Sinks;
using Xunit;

namespace Dreamine.Logging.Tests;

public sealed class LogSinkTests
{
    [Fact]
    public void Composite_sink_validates_configuration()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeLogSink(null!));
        Assert.Throws<ArgumentException>(() => new CompositeLogSink([]));
    }

    [Fact]
    public void Composite_sink_isolates_failures_and_disposes_children_once()
    {
        var recording = new RecordingSink();
        var throwing = new ThrowingDisposableSink();
        var sink = new CompositeLogSink([throwing, recording]);
        var entry = Entry("message");

        Assert.Throws<ArgumentNullException>(() => sink.Write(null!));
        sink.Write(entry);
        sink.Dispose();
        sink.Dispose();
        sink.Write(Entry("ignored"));

        Assert.Same(entry, Assert.Single(recording.Entries));
        Assert.Equal(1, recording.DisposeCount);
        Assert.Equal(1, throwing.DisposeCount);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Async_sink_rejects_non_positive_settings(int capacity, int batchSize)
    {
        var inner = new RecordingSink();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AsyncQueueSink(inner, capacity, batchSize));
    }

    [Fact]
    public void Async_sink_rejects_null_inner_sink()
    {
        Assert.Throws<ArgumentNullException>(() => new AsyncQueueSink(null!));
    }

    [Fact]
    public async Task Async_sink_forwards_entries_and_disposes_inner_sink()
    {
        var inner = new RecordingSink(expectedWrites: 2);
        var sink = new AsyncQueueSink(inner, capacity: 4, drainBatchSize: 2);
        var first = Entry("first");
        var second = Entry("second");

        Assert.Throws<ArgumentNullException>(() => sink.Write(null!));
        sink.Write(first);
        sink.Write(second);
        await inner.WaitForWritesAsync();
        await sink.ShutdownAsync(TimeSpan.FromSeconds(2));
        await sink.ShutdownAsync(TimeSpan.FromSeconds(2));
        sink.Write(Entry("ignored"));

        Assert.Equal([first, second], inner.Entries);
        Assert.Equal(1, inner.DisposeCount);
        Assert.Equal(0, sink.DroppedCount);
    }

    [Fact]
    public async Task Async_sink_survives_inner_write_and_dispose_failures()
    {
        var inner = new ThrowingAsyncSink();
        await using var sink = new AsyncQueueSink(inner, capacity: 2, drainBatchSize: 1);

        sink.Write(Entry("failure"));
        await inner.WriteAttempted.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Formatter_and_options_expose_expected_behavior()
    {
        var exception = new InvalidOperationException("boom");
        var entry = new DreamineLogEntry(
            new DateTimeOffset(2026, 7, 31, 1, 2, 3, 4, TimeSpan.Zero),
            DreamineLogLevel.Error,
            "tests",
            "failed",
            exception,
            7);
        var formatter = new DreamineTextLogFormatter();
        var options = new DreamineLoggingOptions();

        var text = formatter.Format(entry);

        Assert.Contains("[2026-07-31 01:02:03.004] [Error] [tests] [T7] failed", text);
        Assert.Contains("InvalidOperationException", text);
        Assert.Throws<ArgumentNullException>(() => formatter.Format(null!));
        Assert.Equal("Dreamine", options.Category);
        Assert.Equal(1000, options.StoreCapacity);
        Assert.Equal(8192, options.QueueCapacity);
        Assert.Equal(256, options.DrainBatchSize);
        Assert.Equal(20, options.FlushEveryWriteCount);
        Assert.Equal(TimeSpan.FromSeconds(2), options.ShutdownTimeout);
        Assert.EndsWith("Logs", options.LogDirectory);
    }

    private static DreamineLogEntry Entry(string message) =>
        new(DateTimeOffset.UtcNow, DreamineLogLevel.Info, "tests", message, null, 1);

    private sealed class RecordingSink(int expectedWrites = 0) : IDreamineLogSink, IDisposable
    {
        private readonly TaskCompletionSource _writesCompleted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<DreamineLogEntry> Entries { get; } = [];
        public int DisposeCount { get; private set; }

        public void Write(DreamineLogEntry entry)
        {
            lock (Entries)
            {
                Entries.Add(entry);
                if (expectedWrites > 0 && Entries.Count >= expectedWrites)
                    _writesCompleted.TrySetResult();
            }
        }

        public Task WaitForWritesAsync() =>
            _writesCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        public void Dispose() => DisposeCount++;
    }

    private sealed class ThrowingDisposableSink : IDreamineLogSink, IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Write(DreamineLogEntry entry) => throw new InvalidOperationException();

        public void Dispose()
        {
            DisposeCount++;
            throw new InvalidOperationException();
        }
    }

    private sealed class ThrowingAsyncSink : IDreamineLogSink, IAsyncDisposable
    {
        public TaskCompletionSource WriteAttempted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Write(DreamineLogEntry entry)
        {
            WriteAttempted.TrySetResult();
            throw new InvalidOperationException();
        }

        public ValueTask DisposeAsync() => ValueTask.FromException(new InvalidOperationException());
    }
}
