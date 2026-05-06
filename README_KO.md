# Dreamine.Logging

Dreamine.Logging은 Dreamine 애플리케이션을 위한 핵심 로그 인프라 패키지입니다.
Logger 추상화, 로그 엔트리 모델, 메모리 로그 저장소, 텍스트 포매터, 복합 Sink, 일자별 텍스트 파일 저장 기능을 제공합니다.

[➡️ English Version](./README.md)

## 목적

이 패키지는 WPF 또는 특정 UI 프레임워크에 의존하지 않는 순수 로그 Core 계층입니다.
Dreamine 기반 애플리케이션에서 가볍고 확장 가능한 로그 파이프라인을 구성하는 것을 목표로 합니다.

## 주요 기능

- `IDreamineLogger` 추상화
- `Trace`, `Debug`, `Info`, `Warning`, `Error`, `Fatal` 로그 레벨
- 구조화된 로그 데이터 모델 `DreamineLogEntry`
- 런타임 진단 및 UI 연동을 위한 `InMemoryLogStore`
- 로그 출력 대상 확장을 위한 `IDreamineLogSink`
- 여러 출력 대상으로 동시에 기록하는 `CompositeLogSink`
- 일반 텍스트 변환을 위한 `DreamineTextLogFormatter`
- 일자별 파일 저장을 위한 `TextFileLogSink`

## 기본 구조

```text
IDreamineLogger
  -> CompositeLogSink
     -> InMemoryLogStore
     -> TextFileLogSink
```

## 등록 예시

```csharp
var logStore = new InMemoryLogStore();
var formatter = new DreamineTextLogFormatter();

var textFileSink = new TextFileLogSink(
    Path.Combine(AppContext.BaseDirectory, "Logs"),
    formatter);

var compositeSink = new CompositeLogSink(new IDreamineLogSink[]
{
    logStore,
    textFileSink
});

DMContainer.RegisterSingleton<IDreamineLogStore>(logStore);
DMContainer.RegisterSingleton<IDreamineLogSink>(compositeSink);
DMContainer.RegisterSingleton<IDreamineLogFormatter>(formatter);
DMContainer.RegisterSingleton<IDreamineLogger>(
    new DreamineLogger(compositeSink, "SampleSmart"));
```

## 로그 파일 출력

`TextFileLogSink`를 사용하면 로그는 날짜별 파일로 저장됩니다.

```text
Logs/yyyy-MM-dd.log
```

예시:

```text
[2026-05-05 23:00:22.718] [Info] [SampleSmart] [T1] PageLog requested.
```

## 스레드 안전성

`TextFileLogSink`는 내부 lock을 사용하여 동일 인스턴스 기준 파일 쓰기를 직렬화합니다.
일반적인 WPF 애플리케이션에서는 Sink를 한 번만 등록하고 `DMContainer`를 통해 공유하면 충분합니다.

다만 초당 대량 로그, 다중 프로세스 로그, 외부 서비스 연동이 필요한 경우에는 비동기 큐 기반 Dispatcher 또는 별도 로그 서비스를 고려해야 합니다.

## 권장 사용 방식

이 패키지는 UI에 의존하지 않아야 합니다.
WPF 로그 패널은 `Dreamine.Logging.Wpf` 패키지에서 제공합니다.

## 향후 계획

- `DreamineLoggerOptions`
- `DMContainer.UseDreamineLogging(...)`
- 비동기 큐 기반 로그 Dispatcher
- 로그 파일 롤링 정책
- Database Sink
- 외부 로그 서비스 Sink
- 애플리케이션 종료 시 Flush 처리

## 라이선스

MIT License
