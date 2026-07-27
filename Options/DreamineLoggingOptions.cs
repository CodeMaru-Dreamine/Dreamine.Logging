using System;

namespace Dreamine.Logging.Options;

/// <summary>
/// \if KO
/// <para>Dreamine 로깅 등록 구성 옵션을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides configuration options for Dreamine logging registration.</para>
/// \endif
/// </summary>
public sealed class DreamineLoggingOptions
{
    /// <summary>
    /// \if KO
    /// <para>로그 범주 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the log category name.</para>
    /// \endif
    /// </summary>
    public string Category { get; set; } = "Dreamine";

    /// <summary>
    /// \if KO
    /// <para>로그 디렉터리 경로를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the log directory path.</para>
    /// \endif
    /// </summary>
    public string LogDirectory { get; set; } =
        System.IO.Path.Combine(AppContext.BaseDirectory, "Logs");

    /// <summary>
    /// \if KO
    /// <para>메모리에 유지할 최대 로그 항목 수를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the maximum number of log entries kept in memory.</para>
    /// \endif
    /// </summary>
    public int StoreCapacity { get; set; } = 1000;

    /// <summary>
    /// \if KO
    /// <para>비동기 큐 용량을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the asynchronous queue capacity.</para>
    /// \endif
    /// </summary>
    public int QueueCapacity { get; set; } = 8192;

    /// <summary>
    /// \if KO
    /// <para>한 배치에서 처리할 항목 수를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the number of entries drained per batch.</para>
    /// \endif
    /// </summary>
    public int DrainBatchSize { get; set; } = 256;

    /// <summary>
    /// \if KO
    /// <para>텍스트 파일 출력을 flush하기 전 기록 횟수를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the number of writes before flushing the text-file sink.</para>
    /// \endif
    /// </summary>
    public int FlushEveryWriteCount { get; set; } = 20;

    /// <summary>
    /// \if KO
    /// <para>대기 중 로그를 처리하며 종료할 때 사용할 제한 시간을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the shutdown timeout used to drain pending log entries.</para>
    /// \endif
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(2);
}
