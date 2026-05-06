using System;
using System.Text;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Formatters
{
    /// <summary>
    /// Formats Dreamine log entries as plain text.
    /// </summary>
    public sealed class DreamineTextLogFormatter : IDreamineLogFormatter
    {
        /// <inheritdoc />
        public string Format(DreamineLogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            var builder = new StringBuilder();

            builder.Append('[')
                .Append(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .Append("] [")
                .Append(entry.Level)
                .Append("] [")
                .Append(entry.Category)
                .Append("] [T")
                .Append(entry.ThreadId)
                .Append("] ")
                .Append(entry.Message);

            if (entry.Exception is not null)
            {
                builder.AppendLine();
                builder.Append(entry.Exception);
            }

            return builder.ToString();
        }
    }
}