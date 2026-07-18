using System;
using System.IO;
using System.Text;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Sinks
{
    /// <summary>
    /// \if KO
    /// <para>Dreamine 로그 항목을 날짜별 텍스트 로그 파일에 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes Dreamine log entries to daily text log files.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>현재 날짜 파일을 열린 상태로 유지하고 지정 횟수마다 또는 오류 이상에서 즉시 flush하며 날짜 변경을 자동 처리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Keeps the current daily file open, flushes periodically or immediately for errors, and handles date rollover automatically.</para>
    /// \endif
    /// </remarks>
    public sealed class TextFileLogSink : IDreamineLogSink, IDisposable
    {
        /// <summary>
        /// \if KO
        /// <para>Default Flush Every Write Count 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the default flush every write count value.</para>
        /// \endif
        /// </summary>
        private const int DefaultFlushEveryWriteCount = 20;

        /// <summary>
        /// \if KO
        /// <para>sync Root 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the sync root value.</para>
        /// \endif
        /// </summary>
        private readonly object _syncRoot = new();
        /// <summary>
        /// \if KO
        /// <para>log Directory 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the log directory value.</para>
        /// \endif
        /// </summary>
        private readonly string _logDirectory;
        /// <summary>
        /// \if KO
        /// <para>formatter 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the formatter value.</para>
        /// \endif
        /// </summary>
        private readonly IDreamineLogFormatter _formatter;
        /// <summary>
        /// \if KO
        /// <para>flush Every Write Count 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the flush every write count value.</para>
        /// \endif
        /// </summary>
        private readonly int _flushEveryWriteCount;
        /// <summary>
        /// \if KO
        /// <para>writer 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the writer value.</para>
        /// \endif
        /// </summary>
        private StreamWriter? _writer;
        /// <summary>
        /// \if KO
        /// <para>current File Path 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the current file path value.</para>
        /// \endif
        /// </summary>
        private string? _currentFilePath;
        /// <summary>
        /// \if KO
        /// <para>pending Flush Count 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the pending flush count value.</para>
        /// \endif
        /// </summary>
        private int _pendingFlushCount;
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
        /// <para>로그 디렉터리, 포맷터 및 flush 주기로 <see cref="T:Dreamine.Logging.Sinks.TextFileLogSink" />를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes <see cref="T:Dreamine.Logging.Sinks.TextFileLogSink" /> with a directory, formatter, and flush interval.</para>
        /// \endif
        /// </summary>
        /// <param name="logDirectory">
        /// \if KO
        /// <para>로그 디렉터리 경로이며 비어 있으면 Logs를 사용합니다.</para>
        /// \endif
        /// \if EN
        /// <para>The log directory path; Logs is used when empty.</para>
        /// \endif
        /// </param>
        /// <param name="formatter">
        /// \if KO
        /// <para>로그 포맷터입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The log formatter.</para>
        /// \endif
        /// </param>
        /// <param name="flushEveryWriteCount">
        /// \if KO
        /// <para>flush 사이의 기록 횟수이며 0 이하면 20을 사용합니다. 오류 이상은 즉시 flush합니다.</para>
        /// \endif
        /// \if EN
        /// <para>The write count between flushes; 20 is used when non-positive, and errors are always flushed immediately.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="formatter"/>가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="formatter"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
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

        /// <summary>
        /// \if KO
        /// <para>항목을 해당 날짜 파일에 기록하고 구성된 조건에서 flush합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Writes an entry to its daily file and flushes under the configured conditions.</para>
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

        /// <summary>
        /// \if KO
        /// <para>대기 중 내용을 flush하고 파일 작성기를 해제합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Flushes pending content and releases the file writer.</para>
        /// \endif
        /// </summary>
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

        /// <summary>
        /// \if KO
        /// <para>로그 디렉터리를 준비하고 타임스탬프에 해당하는 날짜별 파일 경로를 만듭니다.</para>
        /// \endif
        /// \if EN
        /// <para>Ensures the log directory exists and builds the daily file path for a timestamp.</para>
        /// \endif
        /// </summary>
        /// <param name="timestamp">
        /// \if KO
        /// <para>로그 타임스탬프입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The log timestamp.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>날짜별 로그 파일 경로입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The daily log file path.</para>
        /// \endif
        /// </returns>
        private string GetFilePath(DateTimeOffset timestamp)
        {
            Directory.CreateDirectory(_logDirectory);
            return Path.Combine(_logDirectory, $"{timestamp:yyyy-MM-dd}.log");
        }

        /// <summary>
        /// \if KO
        /// <para>지정한 파일 경로를 대상으로 하는 작성기를 준비하고 날짜 변경 시 교체합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Ensures a writer targets the specified path and replaces it after date rollover.</para>
        /// \endif
        /// </summary>
        /// <param name="filePath">
        /// \if KO
        /// <para>대상 파일 경로입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The target file path.</para>
        /// \endif
        /// </param>
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

        /// <summary>
        /// \if KO
        /// <para>현재 작성기의 대기 내용을 파일에 반영하고 기록 카운터를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Flushes pending writer content and resets the write counter.</para>
        /// \endif
        /// </summary>
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
