using System;
using System.IO;
using System.Text;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Sinks
{
    /// <summary>
    /// Writes Dreamine log entries to daily text log files.
    /// </summary>
    /// <remarks>
    /// Keeps the current daily log file open across writes instead of opening and
    /// closing a file per entry. Flushes either every <c>flushEveryWriteCount</c>
    /// entries or immediately when an entry has level <see cref="DreamineLogLevel.Error"/>
    /// or higher, so important diagnostics are persisted promptly. Date rollover
    /// is detected automatically. Thread-safe; recommended placement is behind
    /// <see cref="AsyncQueueSink"/> so file I/O runs on a single background worker.
    /// </remarks>
    public sealed class TextFileLogSink : IDreamineLogSink, IDisposable
    {
        private const int DefaultFlushEveryWriteCount = 20;

        private readonly object _syncRoot = new();
        private readonly string _logDirectory;
        private readonly IDreamineLogFormatter _formatter;
        private readonly int _flushEveryWriteCount;
        private StreamWriter? _writer;
        private string? _currentFilePath;
        private int _pendingFlushCount;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFileLogSink"/> class.
        /// </summary>
        /// <param name="logDirectory">The log directory path.</param>
        /// <param name="formatter">The log formatter.</param>
        /// <param name="flushEveryWriteCount">
        /// The number of writes between flush operations. Defaults to 20.
        /// Entries with level <see cref="DreamineLogLevel.Error"/> or higher
        /// are always flushed immediately regardless of this value.
        /// </param>
        public TextFileLogSink(
            string logDirectory,
            IDreamineLogFormatter formatter,
            int flushEveryWriteCount = DefaultFlushEveryWriteCount)
        {
            _logDirectory = string.IsNullOrWhiteSpace(logDirectory)
                ? "Logs"
                : logDirectory;

            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _flushEveryWriteCount = flushEveryWriteCount <= 0
                ? DefaultFlushEveryWriteCount
                : flushEveryWriteCount;
        }

        /// <inheritdoc />
        public void Write(DreamineLogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            lock (_syncRoot)
            {
                if (_disposed)
                {
                    return;
                }

                var filePath = GetFilePath(entry.Timestamp);
                EnsureWriter(filePath);

                var text = _formatter.Format(entry);

                _writer!.WriteLine(text);
                _pendingFlushCount++;

                if (_pendingFlushCount >= _flushEveryWriteCount
                    || entry.Level >= DreamineLogLevel.Error)
                {
                    FlushCore();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    FlushCore();
                }
                catch
                {
                    // Suppress flush errors during dispose.
                }

                _writer?.Dispose();
                _writer = null;
                _disposed = true;
            }
        }

        private string GetFilePath(DateTimeOffset timestamp)
        {
            Directory.CreateDirectory(_logDirectory);
            return Path.Combine(_logDirectory, $"{timestamp:yyyy-MM-dd}.log");
        }

        private void EnsureWriter(string filePath)
        {
            if (_writer is not null
                && string.Equals(_currentFilePath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Date rolled over (or first write) — close the previous file and open the new one.
            FlushCore();
            _writer?.Dispose();

            _currentFilePath = filePath;
            _writer = new StreamWriter(
                new FileStream(
                    filePath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite),
                Encoding.UTF8)
            {
                AutoFlush = false
            };
        }

        private void FlushCore()
        {
            if (_writer is null)
            {
                _pendingFlushCount = 0;
                return;
            }

            _writer.Flush();
            _pendingFlushCount = 0;
        }
    }
}
