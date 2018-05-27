using System;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// Convenience class to use a stopwatch with a debug log with frequent resetting.
/// </summary>
public class RBStopwatch
{
    private string name;
    private Stopwatch stopwatch;
    private bool disableLogs;

    private Dictionary<string, TimeSpan> timeRecords;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:RBStopwatch"/> class.
    /// </summary>
    /// <param name="name">Name.</param>
    public RBStopwatch(string name, bool disableLog)
    {
        this.name = name;
        this.stopwatch = new Stopwatch();
        this.stopwatch.Start();

        this.timeRecords = new Dictionary<string, TimeSpan>();
        this.disableLogs = disableLog;
    }
    
    /// <summary>
    /// Logs the time elapsed on the stopwatch since the last time it was constructed or Logged.
    /// </summary>
    /// <param name="logTag">Log tag.</param>
    public void LogLap(string logTag)
    {
        this.LogTimespan(logTag, stopwatch.Elapsed, stopwatch.ElapsedTicks);
        this.RestartTimer();
    }

    public void AddTimeRecord(string key)
    {
        if (this.timeRecords.ContainsKey(key))
        {
            this.timeRecords[key] += this.stopwatch.Elapsed;
        }
        else
        {
            this.timeRecords.Add(key, this.stopwatch.Elapsed);
        }

        this.RestartTimer();
    }

    public void LogTimeRecord(string key)
    {
        var timespan = this.GetTimeRecord(key);
        this.LogTimespan(key, timespan, this.stopwatch.ElapsedTicks);
    }

    private void LogTimespan(string logTag, TimeSpan timeSpan, long ticks)
    {
        if (!this.disableLogs)
        {
            var logFormatString = string.Concat("[", this.name, "] ", logTag, " elapsed: {0} min, {1} sec, {2} milliseconds. Ticks: {3}");
            UnityEngine.Debug.LogFormat(null, logFormatString, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds, ticks);
        }
    }

    private void RestartTimer()
    {
        this.stopwatch.Stop();
        this.stopwatch.Reset();
        this.stopwatch.Start();
    }

    private TimeSpan GetTimeRecord(string key)
    {
        if (this.timeRecords.ContainsKey(key))
        {
            return this.timeRecords[key];
        }
        else
        {
            return TimeSpan.Zero;
        }
    }
}