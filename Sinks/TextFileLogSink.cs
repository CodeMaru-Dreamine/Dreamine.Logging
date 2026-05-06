using System;
using System.IO;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Sinks
{
    /// <summary>
    /// Writes Dreamine log entries to daily text log files.
    /// </summary>
    public sealed class TextFileLogSink : IDreamineLogSink
    {
        private readonly object _syncRoot = new();
        private readonly string _logDirectory;
        private readonly IDreamineLogFormatter _formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFileLogSink"/> class.
        /// </summary>
        /// <param name="logDirectory">The log directory path.</param>
        /// <param name="formatter">The log formatter.</param>
        public TextFileLogSink(string logDirectory, IDreamineLogFormatter formatter)
        {
            _logDirectory = string.IsNullOrWhiteSpace(logDirectory)
                ? "Logs"
                : logDirectory;

            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        /// <inheritdoc />
        public void Write(DreamineLogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            Directory.CreateDirectory(_logDirectory);

            var filePath = Path.Combine(
                _logDirectory,
                $"{entry.Timestamp:yyyy-MM-dd}.log");

            var text = _formatter.Format(entry);

            lock (_syncRoot)
            {
                File.AppendAllText(filePath, text + Environment.NewLine);
            }
        }
    }
}