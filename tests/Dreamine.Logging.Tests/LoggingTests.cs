using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;
using Dreamine.Logging.Services;
using Xunit;

namespace Dreamine.Logging.Tests;

public sealed class LoggingTests
{
    [Fact]
    public void InMemoryStoreKeepsNewestEntriesWithinCapacity()
    {
        var store = new InMemoryLogStore(2);
        var raised = new List<DreamineLogEntry>();
        store.LogAdded += (_, entry) => raised.Add(entry);

        store.Write(Entry("one"));
        store.Write(Entry("two"));
        store.Write(Entry("three"));

        Assert.Equal(["two", "three"], store.GetEntries().Select(entry => entry.Message));
        Assert.Equal(3, raised.Count);

        store.Clear();
        Assert.Empty(store.GetEntries());
    }

    [Fact]
    public void LoggerFiltersLevelsAndNormalizesBlankCategory()
    {
        var sink = new RecordingSink();
        var logger = new DreamineLogger(sink, DreamineLogLevel.Warning, " ");

        logger.Info("ignored");
        logger.Warning("kept");
        logger.Error(new InvalidOperationException("failure"), "broken");

        Assert.Equal(2, sink.Entries.Count);
        Assert.All(sink.Entries, entry => Assert.Equal("Dreamine", entry.Category));
        Assert.Equal(DreamineLogLevel.Warning, sink.Entries[0].Level);
        Assert.IsType<InvalidOperationException>(sink.Entries[1].Exception);
    }

    [Fact]
    public void LoggerContinuesWhenOneSinkFails()
    {
        var recording = new RecordingSink();
        var logger = new DreamineLogger(
            [new ThrowingSink(), recording],
            DreamineLogLevel.Trace,
            "tests");

        logger.Debug("survives");

        Assert.Single(recording.Entries);
        Assert.Equal("survives", recording.Entries[0].Message);
    }

    [Fact]
    public void EntryDisplayTextIncludesMetadataAndException()
    {
        var timestamp = new DateTimeOffset(2026, 7, 30, 9, 8, 7, 123, TimeSpan.Zero);
        var entry = new DreamineLogEntry(
            timestamp,
            DreamineLogLevel.Error,
            "worker",
            "failed",
            new InvalidOperationException("boom"),
            12);

        Assert.Contains("[2026-07-30 09:08:07.123]", entry.DisplayText);
        Assert.Contains("[Error] [worker] [T12] failed", entry.DisplayText);
        Assert.Contains("InvalidOperationException", entry.DisplayText);
    }

    [Fact]
    public void LoggerRejectsMissingSinks()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DreamineLogger((IEnumerable<IDreamineLogSink>)null!));
        Assert.Throws<ArgumentException>(
            () => new DreamineLogger(Array.Empty<IDreamineLogSink>()));
    }

    private static DreamineLogEntry Entry(string message) =>
        new(DateTimeOffset.UtcNow, DreamineLogLevel.Info, "tests", message, null, 1);

    private sealed class RecordingSink : IDreamineLogSink
    {
        public List<DreamineLogEntry> Entries { get; } = [];

        public void Write(DreamineLogEntry entry) => Entries.Add(entry);
    }

    private sealed class ThrowingSink : IDreamineLogSink
    {
        public void Write(DreamineLogEntry entry) => throw new InvalidOperationException();
    }
}
