using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Sg4Mvc.Generator;

public static class Logging
{
    public const String Debug = "SG4DEBUG";

    public static String LogDirectory { get; set; }

    private static List<String> LogEntries { get; } = new List<String>();
    private static List<(String Description, Int64 elapsed)> StopwatchReports { get; } = new();

    [Conditional(Debug)]
    public static void WriteFile()
    {
        foreach (var s in StopwatchReports)
        {
            var percentOfTotal = s.elapsed / Total.TotalMilliseconds;

            var logEntry = $"{s.elapsed,8} ms  {percentOfTotal,5:P1}  {s.Description}";

            LogEntries.Add(logEntry);
        }

        LogEntries.Add($"Total: {Total}");
        LogEntries.Add(String.Empty);
        File.Delete(LogDirectory + "/Sg4Mvc.log");
        File.WriteAllLines(LogDirectory + "/Sg4Mvc.log", LogEntries);
    }

    [Conditional(Debug)]
    public static void Log(String logEntry)
    {
        LogEntries.Add(logEntry);
    }

    private static TimeSpan Total { get; set; } = TimeSpan.Zero;
    private static Stopwatch InternalStopwatch { get; } = Stopwatch.StartNew();

    [Conditional(Debug)]
    public static void ReportProgress(String description)
    {
        InternalStopwatch.Stop();
        ReportStopwatch(description, InternalStopwatch);
        Total += InternalStopwatch.Elapsed;
        InternalStopwatch.Restart();
    }

    [Conditional(Debug)]
    public static void ReportStopwatch(String description, Stopwatch sw)
    {
        StopwatchReports.Add((description, sw.ElapsedMilliseconds));
    }
}
