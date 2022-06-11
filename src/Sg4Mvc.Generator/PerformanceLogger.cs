using System;
using System.Diagnostics;

namespace Sg4Mvc.Generator;

public class PerformanceLogger : IDisposable
{
    public PerformanceLogger(String description)
    {
        Description = description;
        Stopwatch = Stopwatch.StartNew();
    }

    private String Description { get; }
    private Stopwatch Stopwatch { get; }

    public void Dispose()
    {
        Logging.ReportStopwatch(Description, Stopwatch);
    }
}
