using System;
using System.Collections.Generic;
using System.Linq;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Sinks
{
    /// <summary>
    /// \if KO
    /// <para>로그 항목을 여러 출력에 전달합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes log entries to multiple log sinks.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>복합 출력을 해제하면 <see cref="IDisposable"/>을 구현한 내부 출력도 함께 해제됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Disposing the composite also disposes inner sinks that implement <see cref="IDisposable"/>.</para>
    /// \endif
    /// </remarks>
    public sealed class CompositeLogSink : IDreamineLogSink, IDisposable
    {
        /// <summary>
        /// \if KO
        /// <para>sinks 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the sinks value.</para>
        /// \endif
        /// </summary>
        private readonly IReadOnlyList<IDreamineLogSink> _sinks;
        /// <summary>
        /// \if KO
        /// <para>disposed 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the disposed value.</para>
        /// \endif
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// \if KO
        /// <para>지정한 출력 시퀀스로 <see cref="T:Dreamine.Logging.Sinks.CompositeLogSink" />를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes <see cref="T:Dreamine.Logging.Sinks.CompositeLogSink" /> with the specified sinks.</para>
        /// \endif
        /// </summary>
        /// <param name="sinks">
        /// \if KO
        /// <para>로그 출력 시퀀스입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The log sinks.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="sinks"/>가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="sinks"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        /// <exception cref="ArgumentException">
        /// \if KO
        /// <para>출력 시퀀스가 비어 있는 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when the sink sequence is empty.</para>
        /// \endif
        /// </exception>
        public CompositeLogSink(IEnumerable<IDreamineLogSink> sinks)
        {
            ArgumentNullException.ThrowIfNull(sinks);

            _sinks = sinks.ToArray();

            if (_sinks.Count == 0)
            {
                throw new ArgumentException("At least one log sink is required.", nameof(sinks));
            }
        }

        /// <summary>
        /// \if KO
        /// <para>항목을 모든 내부 출력에 기록하며 출력별 실패를 격리합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Writes an entry to all inner sinks while isolating individual failures.</para>
        /// \endif
        /// </summary>
        /// <param name="entry">
        /// \if KO
        /// <para>기록할 항목입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The entry to write.</para>
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
        /// \if KO
        /// <para><see cref="T:System.IDisposable" />을 구현한 모든 내부 출력을 해제합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Disposes inner sinks that implement <see cref="T:System.IDisposable" />.</para>
        /// \endif
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
