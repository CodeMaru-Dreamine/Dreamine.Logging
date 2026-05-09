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
    /// <remarks>
    /// Implements <see cref="IDisposable"/> so that disposing the composite
    /// disposes any inner sinks that hold unmanaged resources (e.g. file
    /// handles in <see cref="TextFileLogSink"/>). This allows the outermost
    /// owner (typically <see cref="AsyncQueueSink"/>) to cleanly release
    /// the entire chain through a single Dispose call.
    /// </remarks>
    public sealed class CompositeLogSink : IDreamineLogSink, IDisposable
    {
        private readonly IReadOnlyList<IDreamineLogSink> _sinks;
        private bool _disposed;

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

            if (_disposed)
            {
                return;
            }

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

        /// <summary>
        /// Disposes any inner sinks that implement <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var sink in _sinks)
            {
                if (sink is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // Logging dispose failure must not terminate the application.
                    }
                }
            }
        }
    }
}
