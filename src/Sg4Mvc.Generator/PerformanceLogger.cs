using System;
using System.Diagnostics;

namespace Sg4Mvc.Generator;

public class PerformanceLogger(String description) : IDisposable
{
    private Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

    public void Dispose()
    {
        Logging.ReportStopwatch(description, Stopwatch);
    }
}
