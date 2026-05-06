using System;
using System.Collections.Generic;
using System.Linq;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Sinks
{
    /// <summary>
    /// Writes log entries to multiple log sinks.
    /// </summary>
    public sealed class CompositeLogSink : IDreamineLogSink
    {
        private readonly IReadOnlyList<IDreamineLogSink> _sinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeLogSink"/> class.
        /// </summary>
        /// <param name="sinks">The log sinks.</param>
        public CompositeLogSink(IEnumerable<IDreamineLogSink> sinks)
        {
            ArgumentNullException.ThrowIfNull(sinks);

            _sinks = sinks.ToArray();

            if (_sinks.Count == 0)
            {
                throw new ArgumentException("At least one log sink is required.", nameof(sinks));
            }
        }

        /// <inheritdoc />
        public void Write(DreamineLogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Write(entry);
                }
                catch
                {
                    // Logging failure must not terminate the application.
                }
            }
        }
    }
}