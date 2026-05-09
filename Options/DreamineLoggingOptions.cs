using System;

namespace Dreamine.Logging.Options;

/// <summary>
/// Provides configuration options for Dreamine logging registration.
/// </summary>
public sealed class DreamineLoggingOptions
{
    /// <summary>
    /// Gets or sets the log category name.
    /// </summary>
    public string Category { get; set; } = "Dreamine";

    /// <summary>
    /// Gets or sets the log directory path.
    /// </summary>
    public string LogDirectory { get; set; } =
        System.IO.Path.Combine(AppContext.BaseDirectory, "Logs");

    /// <summary>
    /// Gets or sets the maximum number of log entries kept in memory.
    /// </summary>
    public int StoreCapacity { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the async queue capacity.
    /// </summary>
    public int QueueCapacity { get; set; } = 8192;

    /// <summary>
    /// Gets or sets the number of entries drained per batch.
    /// </summary>
    public int DrainBatchSize { get; set; } = 256;

    /// <summary>
    /// Gets or sets the number of writes before flushing the text file sink.
    /// </summary>
    public int FlushEveryWriteCount { get; set; } = 20;

    /// <summary>
    /// Gets or sets the shutdown timeout used to drain pending log entries.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(2);
}