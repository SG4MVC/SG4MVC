using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Sg4Mvc.Generator;

/// <summary>
/// Per-execution logger backed by thread-local storage so concurrent generator
/// executions (multiple projects in the same process) never share state.
/// </summary>
public static class Logging
{
    public const String Debug = "SG4DEBUG";

    // Each generator Execute call runs on its own thread; ThreadStatic prevents
    // cross-project contamination that occurred with the previous static lists.
    [ThreadStatic]
    private static LoggingState _state;

    private static LoggingState State => _state ??= new LoggingState();

    public static String LogDirectory
    {
        get => State.LogDirectory;
        set => State.LogDirectory = value;
    }

    /// <summary>Resets the thread-local state for a fresh execution run.</summary>
    [Conditional(Debug)]
    public static void Reset()
    {
        _state = new LoggingState();
    }

    [Conditional(Debug)]
    public static void WriteFile()
    {
        var s = State;
        var lines = new List<String>();
        foreach (var entry in s.StopwatchReports)
        {
            var percentOfTotal = s.Total.TotalMilliseconds > 0 ? entry.elapsed / s.Total.TotalMilliseconds : 0;
            lines.Add($"{entry.elapsed,8} ms  {percentOfTotal,5:P1}  {entry.Description}");
        }
        lines.Add($"Total: {s.Total}");
        lines.Add(String.Empty);
        lines.AddRange(s.LogEntries);
        lines.Add(String.Empty);
        File.AppendAllLines(s.LogDirectory + "/Sg4Mvc.log", lines);
    }

    [Conditional(Debug)]
    public static void Log(String logEntry) => State.LogEntries.Add(logEntry);

    [Conditional(Debug)]
    public static void ReportProgress(String description)
    {
        var s = State;
        s.InternalStopwatch.Stop();
        ReportStopwatch(description, s.InternalStopwatch);
        s.Total += s.InternalStopwatch.Elapsed;
        s.InternalStopwatch.Restart();
    }

    [Conditional(Debug)]
    public static void ReportStopwatch(String description, Stopwatch sw) =>
        State.StopwatchReports.Add((description, sw.ElapsedMilliseconds));

    private sealed class LoggingState
    {
        public String LogDirectory { get; set; }
        public List<String> LogEntries { get; } = new List<String>();
        public List<(String Description, Int64 elapsed)> StopwatchReports { get; } = new List<(String, Int64)>();
        public TimeSpan Total { get; set; } = TimeSpan.Zero;
        public Stopwatch InternalStopwatch { get; } = Stopwatch.StartNew();
    }
}
