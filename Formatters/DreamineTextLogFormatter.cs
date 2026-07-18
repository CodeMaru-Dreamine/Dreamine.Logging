using System;
using System.Text;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Formatters
{
    /// <summary>
    /// \if KO
    /// <para>Dreamine 로그 항목을 일반 텍스트로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Formats Dreamine log entries as plain text.</para>
    /// \endif
    /// </summary>
    public sealed class DreamineTextLogFormatter : IDreamineLogFormatter
    {
        /// <summary>
        /// \if KO
        /// <para>지정한 로그 항목을 타임스탬프, 수준, 범주, 스레드 및 예외가 포함된 텍스트로 변환합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Formats a log entry as text containing its timestamp, level, category, thread, and exception.</para>
        /// \endif
        /// </summary>
        /// <param name="entry">
        /// \if KO
        /// <para>변환할 로그 항목입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The log entry to format.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>변환된 로그 텍스트입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The formatted log text.</para>
        /// \endif
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="entry"/>가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="entry"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
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
